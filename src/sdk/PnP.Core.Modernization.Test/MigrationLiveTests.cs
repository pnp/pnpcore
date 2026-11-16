using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Transform;
using System;
using System.Security.Cryptography.X509Certificates;

namespace PnP.Core.Modernization.Tests
{
    [TestClass]
    [TestCategory("Live")]
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
            using var authentication = new AuthenticationManager(
                Setting("PNP_MODERNIZATION_CLIENT_ID"), StoreName.My, StoreLocation.CurrentUser,
                Setting("PNP_MODERNIZATION_CERTIFICATE_THUMBPRINT"), Setting("PNP_MODERNIZATION_TENANT_ID"));
            using var source = authentication.GetContext(sourceUrl);
            using var target = authentication.GetContext(targetUrl);

            TestContext.WriteLine($"Modernization assembly: {typeof(PageTransformator).Assembly.Location}");
            TestContext.WriteLine($"Core assembly: {typeof(PnP.Core.Services.PnPContext).Assembly.Location}");
            TestContext.WriteLine($"Framework assembly: {typeof(AuthenticationManager).Assembly.Location}");

            var sourceFile = source.Web.GetFileByServerRelativeUrl(sourcePageUrl);
            source.Load(sourceFile, file => file.TimeLastModified, file => file.ListItemAllFields);
            target.Load(target.Web, web => web.ServerRelativeUrl);
            source.ExecuteQueryRetry();
            target.ExecuteQueryRetry();
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
                target.Load(result, file => file.ListItemAllFields, file => file.Level);
                target.ExecuteQueryRetry();
                Assert.AreEqual(name, result.ListItemAllFields["Title"]);
                Assert.AreEqual(Constants.FeatureId_Web_ModernPage.ToString(),
                    result.ListItemAllFields["ClientSideApplicationId"].ToString());
                Assert.IsFalse(string.IsNullOrWhiteSpace(result.ListItemAllFields["CanvasContent1"]?.ToString()));
                Assert.AreEqual(FileLevel.Draft, result.Level);

                source.Load(sourceFile, file => file.TimeLastModified, file => file.ListItemAllFields);
                source.ExecuteQueryRetry();
                Assert.AreEqual(modified, sourceFile.TimeLastModified, "The existing source must not be modified.");
                Assert.AreEqual(sourceContent, sourceFile.ListItemAllFields[sourceContentField]?.ToString());
            }
            finally
            {
                if (Exists(target, targetPath))
                {
                    target.Web.GetFileByServerRelativeUrl(targetPath).DeleteObject();
                    target.ExecuteQueryRetry();
                }
                Assert.IsFalse(Exists(target, targetPath), "The run-owned destination must be cleaned up.");
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

        private static bool Exists(ClientContext context, string path)
        {
            var file = context.Web.GetFileByServerRelativeUrl(path);
            try
            {
                context.Load(file, value => value.Exists);
                context.ExecuteQueryRetry();
                return file.Exists;
            }
            catch (ServerException exception) when (exception.ServerErrorTypeName == "System.IO.FileNotFoundException")
            {
                return false;
            }
        }
    }
}
