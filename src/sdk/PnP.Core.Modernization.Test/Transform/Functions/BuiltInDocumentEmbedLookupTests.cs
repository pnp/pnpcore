using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Functions;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using PnP.Framework.Utilities.UnitTests.Model;
using PnP.Framework.Utilities.UnitTests.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Xml.Serialization;

namespace PnP.Core.Modernization.Test.Transform.Functions
{
    [TestClass]
    [TestCategory("Offline")]
    public class BuiltInDocumentEmbedLookupTests
    {
        private const string Host = "https://contoso.sharepoint.com";
        private const string SiteUrl = Host + "/sites/document-embed-tests";
        private const string WebServerRelativeUrl = "/sites/document-embed-tests";
        private const string DocumentUrl = WebServerRelativeUrl + "/Documents/missing.docx";
        private const string OtherWebDocumentUrl = "/sites/other/Documents/missing.docx";
        private static readonly Guid DocumentListId = Guid.Parse("f266cd97-f155-4360-b802-47bb9f0d9b55");
        private static readonly Guid DocumentUniqueId = Guid.Parse("a9b5b53e-2014-47d9-b26c-fc8d52047d1e");
        private static readonly string[] MetadataKeys =
        {
            "DocumentWeb", "DocumentListId", "DocumentUniqueId", "DocumentAuthor", "DocumentAuthorName"
        };

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void DocumentEmbedLookup_WhenPathIsEmpty_ReturnsCompleteEmptyMetadata(string path)
        {
            using (var context = CreateContext())
            {
                AssertEmptyMetadata(CreateBuiltIn(context).DocumentEmbedLookup(path));
            }
        }

        [TestMethod]
        public void DocumentEmbedLookup_WhenFileExists_ReturnsDocumentMetadata()
        {
            using (var context = CreateContext())
            {
                var result = CreateBuiltIn(context).DocumentEmbedLookup(DocumentUrl);

                Assert.AreEqual(WebServerRelativeUrl, result["DocumentWeb"]);
                Assert.AreEqual(DocumentListId.ToString(), result["DocumentListId"]);
                Assert.AreEqual(DocumentUniqueId.ToString(), result["DocumentUniqueId"]);
                Assert.AreEqual("author@contoso.com", result["DocumentAuthor"]);
                Assert.AreEqual("Document Author", result["DocumentAuthorName"]);
            }
        }

        [DataTestMethod]
        [DataRow(DocumentUrl)]
        [DataRow(OtherWebDocumentUrl)]
        public void DocumentEmbedLookup_WhenFileIsNotFound_ReturnsMetadataAndLogsReference(string path)
        {
            using (var context = CreateContext())
            {
                var observer = new Mock<ILogObserver>();
                var builtIn = CreateBuiltIn(context, observer.Object);
                SetErrorResponse(context, "System.IO.FileNotFoundException", "File Not Found.", -2147024894);

                AssertEmptyMetadata(builtIn.DocumentEmbedLookup(path));

                observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                    entry.Message.Contains(path) && entry.Message.Contains(SiteUrl) &&
                    entry.Message.Contains("File Not Found") && entry.Message.Contains("access"))), Times.Once);
                observer.Verify(log => log.Error(It.IsAny<LogEntry>()), Times.Never);
            }
        }

        [DataTestMethod]
        [DataRow("System.UnauthorizedAccessException", "Access denied.", -2147024891)]
        [DataRow("Microsoft.SharePoint.SPException", "Unexpected server error.", -1)]
        public void DocumentEmbedLookup_WhenAnotherServerErrorOccurs_LogsAndRethrows(string type, string message, int code)
        {
            using (var context = CreateContext())
            {
                var observer = new Mock<ILogObserver>();
                var builtIn = CreateBuiltIn(context, observer.Object);
                SetErrorResponse(context, type, message, code);

                ServerException exception = type == "System.UnauthorizedAccessException"
                    ? Assert.ThrowsException<ServerUnauthorizedAccessException>(() => builtIn.DocumentEmbedLookup(DocumentUrl))
                    : Assert.ThrowsException<ServerException>(() => builtIn.DocumentEmbedLookup(DocumentUrl));

                Assert.AreEqual(type, exception.ServerErrorTypeName);
                observer.Verify(log => log.Error(It.Is<LogEntry>(entry =>
                    entry.Message.Contains(DocumentUrl) && entry.Exception == exception)), Times.Once);
                observer.Verify(log => log.Warning(It.IsAny<LogEntry>()), Times.Never);
            }
        }

        [DataTestMethod]
        [DataRow("SharePointPnP.Modernization.WikiEmbedPart", "DocumentUrl")]
        [DataRow("Microsoft.Office.Excel.WebUI.ExcelWebRenderer", "WorkbookUri")]
        [DataRow("Microsoft.Office.Visio.Server.WebControls.VisioWebAccess", "DiagramPath")]
        public void DocumentEmbedMapping_WhenFileIsNotFound_ProcessesFunctionsAndPreservesReference(string type, string urlProperty)
        {
            using (var context = CreateContext())
            {
                var transformation = LoadMapping();
                var mapping = transformation.WebParts.Single(part => part.Type.Split(',')[0] == type);
                mapping.Properties = mapping.Properties.Concat(new[]
                {
                    new Property { Name = "Host", Type = PropertyType.@string }
                }).ToArray();
                var webPart = new WebPartEntity { Type = mapping.Type };
                foreach (var property in mapping.Properties)
                {
                    webPart.Properties[property.Name] = string.Empty;
                }
                webPart.Properties[urlProperty] = DocumentUrl;
                webPart.Properties["Host"] = Host;
                webPart.Properties["SiteId"] = "c6815669-f27c-4ff6-b937-0825b2cdfa93";
                webPart.Properties["WebId"] = "9d03e0aa-cb94-4463-b885-15a55d03ed52";

                var processor = new FunctionProcessor(context, context, null, transformation,
                    new PageTransformationInformation(null) { SkipTelemetry = true });
                SetErrorResponse(context, "System.IO.FileNotFoundException", "File Not Found.", -2147024894);

                processor.Process(ref mapping, webPart);

                foreach (var key in MetadataKeys)
                {
                    Assert.AreEqual(string.Empty, webPart.Properties[key], key);
                    Assert.IsTrue(mapping.Properties.Any(property => property.Name == key), key);
                }
                Assert.AreEqual(string.Empty, webPart.Properties["EncodedAuthor"]);
                var json = WebUtility.HtmlDecode(TokenParser.ReplaceTokens(
                    mapping.Mappings.Mapping.Single().ClientSideWebPart.Single().JsonControlData, webPart));
                foreach (var property in mapping.Properties)
                {
                    Assert.IsFalse(json.Contains("{" + property.Name + "}"), property.Name);
                }
                using (var document = JsonDocument.Parse(json))
                {
                    var properties = document.RootElement.GetProperty("properties");
                    Assert.AreEqual(Host + DocumentUrl, properties.GetProperty("file").GetString());
                    Assert.AreEqual(string.Empty, properties.GetProperty("listId").GetString());
                    Assert.AreEqual(string.Empty, properties.GetProperty("uniqueId").GetString());
                    Assert.AreEqual(string.Empty, properties.GetProperty("authorName").GetString());
                    Assert.AreEqual(DocumentUrl, document.RootElement.GetProperty("serverProcessedContent")
                        .GetProperty("links").GetProperty("serverRelativeUrl").GetString());
                }
            }
        }

        private static void AssertEmptyMetadata(Dictionary<string, string> result)
        {
            Assert.IsNotNull(result);
            CollectionAssert.AreEquivalent(MetadataKeys, result.Keys.ToArray());
            foreach (var key in MetadataKeys)
            {
                Assert.AreEqual(string.Empty, result[key], key);
            }
        }

        private static BuiltIn CreateBuiltIn(ClientContext context, ILogObserver observer = null)
        {
            return new BuiltIn(new PageTransformationInformation(null) { SkipTelemetry = true }, context,
                logObservers: observer == null ? null : new[] { observer });
        }

        private static ClientContext CreateContext()
        {
            CacheManager.Instance.SetSharePointVersion(new Uri(SiteUrl), SPVersion.SPO);
            var provider = new MockEntryResponseProvider();
            foreach (var property in new[] { "Site", "Web" })
            {
                provider.ResponseEntries.Add(new MockResponseEntry<object>
                {
                    Url = SiteUrl,
                    PropertyName = property,
                    ReturnValue = new Dictionary<string, object>
                    {
                        ["_ObjectType_"] = "SP." + property,
                        ["ServerRelativeUrl"] = WebServerRelativeUrl,
                        ["Url"] = SiteUrl
                    }
                });
            }
            provider.ResponseEntries.Add(new MockResponseEntry<object>
            {
                Url = SiteUrl,
                Method = "GetFileByServerRelativeUrl",
                ReturnValue = new Dictionary<string, object>
                {
                    ["_ObjectType_"] = "SP.File",
                    ["ListId"] = $"/Guid({DocumentListId})/",
                    ["UniqueId"] = $"/Guid({DocumentUniqueId})/",
                    ["Author"] = new Dictionary<string, object>
                    {
                        ["_ObjectType_"] = "SP.User",
                        ["LoginName"] = "i:0#.f|membership|author@contoso.com",
                        ["Title"] = "Document Author"
                    }
                }
            });
            return new ClientContext(SiteUrl)
            {
                WebRequestExecutorFactory = new MockWebRequestExecutorFactory(provider)
            };
        }

        private static void SetErrorResponse(ClientContext context, string type, string message, int code)
        {
            context.Load(context.Web, web => web.ServerRelativeUrl);
            context.ExecuteQuery();
            var provider = new Mock<IMockResponseProvider>();
            provider.Setup(response => response.GetResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(new[]
                {
                    new
                    {
                        SchemaVersion = "15.0.0.0",
                        LibraryVersion = "16.0.0.0",
                        ErrorInfo = new { ErrorMessage = message, ErrorValue = (string)null, ErrorCode = code, ErrorTypeName = type },
                        TraceCorrelationId = "d991e178-2b53-4ff8-b77f-6bb73516962a"
                    }
                }));
            context.WebRequestExecutorFactory = new MockWebRequestExecutorFactory(provider.Object);
        }

        private static PageTransformation LoadMapping()
        {
            var assembly = typeof(BuiltIn).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .Single(name => name.EndsWith("Modernization.webpartmapping.xml", StringComparison.Ordinal));
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                return (PageTransformation)new XmlSerializer(typeof(PageTransformation)).Deserialize(stream);
            }
        }
    }
}
