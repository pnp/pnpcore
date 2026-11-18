using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

namespace PnP.Core.Modernization.Tests.Transform.Wp
{
    [TestClass]
    [TestCategory("Offline")]
    public class ClassicWebPartReaderTests
    {
        private const string SiteUrl = "https://contoso.sharepoint.com/sites/classic";
        private const string PageUrl = "/sites/classic/SitePages/test.aspx";
        private static readonly Guid PartId = new("dd8fd78b-4bbd-430d-a96a-bfc1f6b77729");

        [TestMethod]
        public void ReadsZoneAndStateAndCachesReadOnlyMetadataAndPermittedExport()
        {
            var reads = 0;
            var exports = 0;
            const string xml = "<WebPart xmlns='http://schemas.microsoft.com/WebPart/v2'><Title>Example</Title></WebPart>";
            using var context = CoreTestContext.Create(SiteUrl, (request, body) =>
            {
                if (request.RequestUri.AbsolutePath.EndsWith("/WebParts", StringComparison.Ordinal))
                {
                    reads++;
                    Assert.AreEqual(HttpMethod.Get, request.Method);
                    StringAssert.Contains(Uri.UnescapeDataString(request.RequestUri.Query), "ZoneId");
                    return CoreTestContext.Json(new { value = new[] { Part(1) } });
                }
                if (request.RequestUri.AbsolutePath.EndsWith("/ExportWebPart", StringComparison.Ordinal))
                {
                    exports++;
                    Assert.AreEqual(HttpMethod.Post, request.Method);
                    using var payload = JsonDocument.Parse(body);
                    Assert.AreEqual(PartId, payload.RootElement.GetProperty("webPartId").GetGuid());
                    return CoreTestContext.Json(new { value = xml });
                }
                Assert.AreEqual(HttpMethod.Get, request.Method);
                return CoreTestContext.Json(new { UniqueId = PartId, ServerRelativeUrl = PageUrl });
            });
            var reader = new ClassicWebPartReader(context.Web.GetFileByServerRelativeUrl(PageUrl));
            var definitions = reader.Read();
            Assert.AreEqual("Main", definitions[0].ZoneId);
            Assert.AreEqual(2, definitions[0].WebPart.ZoneIndex);
            Assert.IsTrue(definitions[0].WebPart.Hidden);
            Assert.IsTrue(definitions[0].WebPart.IsClosed);
            Assert.AreEqual("Text", definitions[0].WebPart.Properties["Content"]);
            Assert.AreSame(definitions, reader.Read());
            Assert.AreEqual(xml, reader.Export(PartId));
            Assert.AreEqual(xml, reader.Export(PartId));
            Assert.AreEqual(1, reads);
            Assert.AreEqual(1, exports);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(2)]
        public void RefusesXmlExportWithoutChangingTheSourceExportMode(int mode)
        {
            using var context = CoreTestContext.Create(SiteUrl, (request, _) =>
            {
                Assert.AreEqual(HttpMethod.Get, request.Method, "No write or export request is permitted.");
                return request.RequestUri.AbsolutePath.EndsWith("/WebParts", StringComparison.Ordinal)
                    ? CoreTestContext.Json(new { value = new[] { Part(mode) } })
                    : CoreTestContext.Json(new { UniqueId = PartId, ServerRelativeUrl = PageUrl });
            });
            var reader = new ClassicWebPartReader(context.Web.GetFileByServerRelativeUrl(PageUrl));
            Assert.ThrowsException<InvalidOperationException>(() => reader.Export(PartId));
        }

        [DataTestMethod]
        [DataRow("https://external.example/_api/web/WebParts")]
        [DataRow("https://contoso.sharepoint.com/sites/another/_api/web/WebParts")]
        public void RejectsContinuationOutsideTheCurrentWeb(string next)
        {
            var calls = 0;
            using var context = CoreTestContext.Create(SiteUrl, (request, _) =>
            {
                if (!request.RequestUri.AbsolutePath.EndsWith("/WebParts", StringComparison.Ordinal))
                    return CoreTestContext.Json(new { UniqueId = PartId, ServerRelativeUrl = PageUrl });
                calls++;
                return CoreTestContext.Json(new Dictionary<string, object>
                {
                    ["value"] = Array.Empty<object>(), ["odata.nextLink"] = next
                });
            });
            var reader = new ClassicWebPartReader(context.Web.GetFileByServerRelativeUrl(PageUrl));
            Assert.ThrowsException<InvalidOperationException>(() => reader.Read());
            Assert.AreEqual(1, calls);
        }

        private static object Part(int exportMode) => new
        {
            Id = PartId, ZoneId = "Main",
            WebPart = new
            {
                Title = "Example", ZoneIndex = 2, Hidden = true, IsClosed = true, ExportMode = exportMode,
                Properties = new { Content = "Text" }
            }
        };
    }
}
