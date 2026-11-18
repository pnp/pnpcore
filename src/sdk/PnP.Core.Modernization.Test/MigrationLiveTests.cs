using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Telemetry.Observers;
using PnP.Core.Modernization.Transform;


using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Tests
{
    [TestClass]
    [TestCategory("Live")]
    [DoNotParallelize]
    public class MigrationLiveTests
    {
        public TestContext TestContext { get; set; }

        [DataTestMethod]
        [DataRow("WIKI", false)]
        [DataRow("PUBLISHING", true)]
        public void TransformCreatesDraftWithoutChangingSource(string scenario, bool publishing)
        {
            if (Environment.GetEnvironmentVariable("PNP_MODERNIZATION_LIVE") != "1")
            {
                Assert.Inconclusive("Set PNP_MODERNIZATION_LIVE=1 to allow scoped live page creation and cleanup.");
            }

            var sourceUrl = Setting($"PNP_MODERNIZATION_{scenario}_SOURCE");
            var sourcePageUrl = Setting($"PNP_MODERNIZATION_{scenario}_PAGE");
            var targetUrl = publishing ? sourceUrl : Setting("PNP_MODERNIZATION_WIKI_TARGET");
            using var source = TestCommon.CreateClientContext(sourceUrl);
            using var target = TestCommon.CreateClientContext(targetUrl);

            TestContext.WriteLine($"Modernization assembly: {typeof(PageTransformator).Assembly.Location}");
            TestContext.WriteLine($"Core assembly: {typeof(PnPContext).Assembly.Location}");
            TestContext.WriteLine($"Core authentication assembly: {typeof(X509CertificateAuthenticationProvider).Assembly.Location}");

            var sourceFile = source.Web.GetFileByServerRelativeUrl(sourcePageUrl);
            sourceFile.LoadBatch(file => file.TimeLastModified, file => file.ListItemAllFields.QueryProperties(item => item.All));
            target.Web.LoadBatch(web => web.ServerRelativeUrl);
            source.Execute();
            target.Execute();
            var modified = sourceFile.TimeLastModified;
            var item = sourceFile.ListItemAllFields;
            var sourceContentField = publishing ? "PublishingPageContent" : "WikiField";
            var sourceContent = item[sourceContentField]?.ToString();
            Assert.IsFalse(string.IsNullOrEmpty(sourceContent), "Select a source fixture containing page content.");
            Assert.IsTrue(sourceContent.IndexOf("<img", StringComparison.OrdinalIgnoreCase) < 0,
                "Select an image-free fixture: this smoke check cleans up only its destination page.");

            var name = $"pnpcore-modernization-smoke-{Guid.NewGuid():N}.aspx";
            var targetPath = $"{target.Web.ServerRelativeUrl.TrimEnd('/')}/SitePages/{name}";
            Assert.IsFalse(Exists(target, targetPath), "The generated destination must not already exist.");
            TestContext.WriteLine($"Owned destination: {targetPath}");
            try
            {
                BaseTransformationInformation information = publishing
                    ? new PublishingPageTransformationInformation(item)
                    : new PageTransformationInformation(item);
                information.TargetPageName = name;
                information.TargetPageFolderOverridesDefaultFolder = true;
                information.TargetPageFolder = "";
                information.PageTitleOverride = _ => name;
                information.Overwrite = false;
                information.PublishCreatedPage = false;
                information.KeepPageSpecificPermissions = false;
                information.KeepPageCreationModificationInformation = false;
                information.SkipUserMapping = true;
                information.SkipTermStoreMapping = true;
                information.SkipTelemetry = true;
                if (publishing)
                {
                    PublishingPageTransformator.CreateInPlace(source)
                        .Transform((PublishingPageTransformationInformation)information);
                }
                else
                {
                    new PageTransformator(source, target).Transform((PageTransformationInformation)information);
                }

                var result = target.Web.GetFileByServerRelativeUrl(targetPath);
                result.LoadBatch(file => file.ListItemAllFields.QueryProperties(item => item.All), file => file.Level);
                target.Execute();
                var response = target.Web.ExecuteRequest(new ApiRequest(HttpMethod.Get, ApiRequestType.SPORest,
                    $"_api/web/GetFileByServerRelativePath(decodedUrl='{Uri.EscapeDataString(targetPath.Replace("'", "''"))}')/ListItemAllFields?$select=Title,ClientSideApplicationId,CanvasContent1",
                    null, new Dictionary<string, string> { ["Accept"] = "application/json;odata=nometadata" }));
                using var resultFields = JsonDocument.Parse(response.Response);
                Assert.AreEqual(name, resultFields.RootElement.GetProperty("Title").GetString());
                Assert.AreEqual(Constants.FeatureId_Web_ModernPage.ToString(),
                    resultFields.RootElement.GetProperty("ClientSideApplicationId").GetString());
                Assert.IsFalse(string.IsNullOrWhiteSpace(resultFields.RootElement.GetProperty("CanvasContent1").GetString()));
                Assert.AreEqual(PublishedStatus.Draft, result.Level);

                sourceFile.LoadBatch(file => file.TimeLastModified, file => file.ListItemAllFields.QueryProperties(item => item.All));
                source.Execute();
                Assert.AreEqual(modified, sourceFile.TimeLastModified, "The existing source must not be modified.");
                Assert.AreEqual(sourceContent, sourceFile.ListItemAllFields[sourceContentField]?.ToString());
            }
            finally
            {
                if (Exists(target, targetPath))
                {
                    target.Web.GetFileByServerRelativeUrl(targetPath).Delete();
                }
                Assert.IsFalse(Exists(target, targetPath), "The run-owned destination must be cleaned up.");
            }
        }

        [TestMethod]
        public async Task CoreReportCreatesModernPageAndCleansUpOwnedFolder()
        {
            if (Environment.GetEnvironmentVariable("PNP_MODERNIZATION_LIVE") != "1")
            {
                Assert.Inconclusive("Set PNP_MODERNIZATION_LIVE=1 to allow scoped report creation and cleanup.");
            }

            var targetUrl = Setting("PNP_MODERNIZATION_WIKI_TARGET");
            using var context = TestCommon.CreateClientContext(targetUrl);
            var pagesPath = $"{new Uri(targetUrl).AbsolutePath.TrimEnd('/')}/SitePages";
            var pages = await context.Web.Lists.GetByServerRelativeUrlAsync(pagesPath, p => p.RootFolder).ConfigureAwait(false);
            var folderName = $"pnpcore-report-smoke-{Guid.NewGuid():N}";
            var folderPath = $"{pagesPath}/{folderName}";
            Assert.IsNull(await pages.RootFolder.Folders.FirstOrDefaultAsync(f => f.Name == folderName).ConfigureAwait(false),
                "The run-owned folder must not already exist.");
            TestContext.WriteLine($"Modernization assembly: {typeof(MarkdownToSharePointObserver).Assembly.Location}");
            TestContext.WriteLine($"Core assembly: {typeof(PnPContext).Assembly.Location}");
            TestContext.WriteLine($"Owned report folder: {folderPath}");
            var report = new MarkdownToSharePointObserver(context.Web, folderName);
            try
            {
                var name = await report.FlushAsync(clearLogData: false).ConfigureAwait(false);
                var file = await context.Web.GetFileByServerRelativeUrlAsync($"{pagesPath}/{name}",
                    f => f.ListItemAllFields).ConfigureAwait(false);
                var response = await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Get, ApiRequestType.SPORest,
                    $"_api/web/lists(guid'{pages.Id}')/items({file.ListItemAllFields.Id})?$select=Title,ClientSideApplicationId,CanvasContent1",
                    null, new Dictionary<string, string> { ["Accept"] = "application/json;odata=nometadata" })).ConfigureAwait(false);
                using var fields = JsonDocument.Parse(response.Response);
                var item = fields.RootElement;
                Assert.AreEqual(LogStrings.Report_ModernisationSummaryReport, item.GetProperty("Title").GetString());
                Assert.AreEqual(Constants.FeatureId_Web_ModernPage.ToString(),
                    item.GetProperty("ClientSideApplicationId").GetString());
                StringAssert.Contains(item.GetProperty("CanvasContent1").GetString(), "Markdown");
            }
            finally
            {
                var folder = await pages.RootFolder.Folders.FirstOrDefaultAsync(f => f.Name == folderName).ConfigureAwait(false);
                if (folder != null)
                {
                    await folder.DeleteAsync().ConfigureAwait(false);
                }
                Assert.IsNull(await pages.RootFolder.Folders.FirstOrDefaultAsync(f => f.Name == folderName).ConfigureAwait(false),
                    "The exact run-owned report folder must be cleaned up.");
            }
        }

        private static string Setting(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                Assert.Inconclusive($"Configure {name} before running this live scenario.");
            }
            return value;
        }

        private static bool Exists(PnPContext context, string path)
        {
            return context.Web.GetFileByServerRelativeUrlOrDefault(path, file => file.UniqueId) != null;
        }
    }
}
