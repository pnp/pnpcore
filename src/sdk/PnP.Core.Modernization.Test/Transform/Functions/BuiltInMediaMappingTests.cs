using AngleSharp.Html.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Functions;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PnP.Core.Modernization.Test.Transform.Functions
{
    [TestClass]
    [TestCategory("Offline")]
    public class BuiltInMediaMappingTests
    {
        private const string SourceUrl = "https://contoso.sharepoint.com/sites/media-source";
        private const string TargetUrl = "https://fabrikam.sharepoint.com/sites/media-target";
        private const string SourcePath = "/sites/media-source/Assets/clip.mp4";
        private const string TargetFolder = "/sites/media-target/SiteAssets/SitePages/clip";
        private const string TargetPath = TargetFolder + "/clip # &quot; {Title}.mp4";
        private const string Embed = "<iframe src=\"https://example.org/embed?x=1&amp;literal=&amp;quot;&amp;token={Title}\" width=\"640\" height=\"360\" allowfullscreen></iframe>";
        private static readonly Guid SourceId = Guid.Parse("2582f2b9-a82a-42b9-a30f-3486cb63a887");
        private static readonly Guid TargetId = Guid.Parse("d865152f-004d-4779-a167-a9c0e5bda2bd");

        [TestMethod]
        public void EmbedWinsWithoutFileOrVideoSetLookupAndHasNoTrustedCache()
        {
            using var source = CreateContext(SourceUrl, out var requests);
            using var target = CreateContext(TargetUrl, out var targetRequests);
            var observer = new Mock<ILogObserver>();
            var json = Transform(source, target, new Dictionary<string, string>
            {
                ["VideoSetEmbedCode"] = Embed, ["MediaSource"] = SourcePath,
                ["VideoSetSource"] = "/missing-video-set"
            }, requests, observer: observer.Object);
            AssertEmbed(json);
            Assert.IsFalse(requests.Concat(targetRequests).Any(request =>
                request.Contains("listitemallfields",StringComparison.OrdinalIgnoreCase)|| request.Contains("GetFile",StringComparison.OrdinalIgnoreCase)|| request.Contains("EnsureSiteAssets",StringComparison.OrdinalIgnoreCase)));
            observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                entry.Message.Contains("HTML Field Security") && entry.Message.Contains("framing"))), Times.Once);
        }

        [DataTestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        public void VideoSetAndContextResolveClassicInputPrecedence(bool fromContext, bool useEmbed)
        {
            var fields = new Dictionary<string, object>
            {
                ["ContentTypeId"] = fromContext ? "0x0101" : "0x0120D520A808",
                ["FileRef"] = SourcePath,
                ["VideoSetEmbedCode"] = Embed
            };
            if (!useEmbed)
            {
                fields["VideoSetUserOverrideEncoding"] = new Dictionary<string, object>
                {
                    ["odata.type"] = "SP.FieldUrlValue", ["Url"] = SourceUrl + "/Assets/clip.mp4",
                    ["Description"] = "Video"
                };
                fields["VideoSetDefaultEncoding"] = SourceUrl + "/incorrect.mp4";
                fields["VideoSetExternalLink"] = "https://external.example/incorrect.mp4";
            }
            using var context = CreateContext(SourceUrl, out var requests, fields);
            IListItem page = null;
            if (fromContext)
            {
                page = CoreTestContext.Item(context);
                foreach (var field in fields) page[field.Key] = field.Value;
            }
            var data = Transform(context, context, new Dictionary<string, string>
            {
                ["ConfigureFromContext"] = fromContext.ToString(), ["VideoSetSource"] = "/sites/media-source/Assets/video-set",
                ["MediaSource"] = fromContext ? "/stale.mp4" : ""
            }, requests, page: page);
            if (useEmbed)
            {
                AssertEmbed(data);
                Assert.IsFalse(requests.Any(request => request.Contains("GetFile",StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                AssertFile(data, SourceUrl, SourcePath, SourceId);
                Assert.IsTrue(requests.Any(request => request.Contains("GetFileBy",StringComparison.OrdinalIgnoreCase)));
            }
        }

        [TestMethod]
        public void DirectFileSkipsVideoSetAndRetainsFileIdentities()
        {
            using var context = CreateContext(SourceUrl, out var requests);
            AssertFile(Transform(context, context, new Dictionary<string, string>
            {
                ["MediaSource"] = "Assets/clip.mp4", ["VideoSetSource"] = "/missing-video-set"
            }, requests), SourceUrl, SourcePath, SourceId);
            Assert.IsFalse(requests.Any(request => request.Contains("listitemallfields",StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void CrossSiteFileUsesTransferredPathAndDestinationIdentities()
        {
            var asset = new AssetTransferredEntity
            {
                SourceAssetUrl = SourcePath, TargetAssetFolderUrl = TargetFolder, TargetAssetTransferredUrl = TargetPath
            };
            CacheManager.Instance.AddAssetTransferredEntity(asset);
            try
            {
                using var source = CreateContext(SourceUrl, out var requests);
                using var target = CreateContext(TargetUrl, out var targetRequests);
                var observer = new Mock<ILogObserver>();
                observer.Setup(log => log.Error(It.IsAny<LogEntry>()))
                    .Callback<LogEntry>(entry => Assert.Fail(entry.Exception?.ToString() ?? entry.Message));
                AssertFile(Transform(source, target, new Dictionary<string, string> { ["MediaSource"] = SourcePath },
                    requests, observer: observer.Object), TargetUrl, TargetPath, TargetId);
                Assert.IsFalse(requests.Any(request => request.Contains("GetFile",StringComparison.OrdinalIgnoreCase)));
                Assert.IsTrue(targetRequests.Any(request => request.Contains("EnsureSiteAssetsLibrary",StringComparison.OrdinalIgnoreCase)));
                Assert.IsTrue(targetRequests.Any(request => request.Contains("GetFileBy",StringComparison.OrdinalIgnoreCase)));
            }
            finally
            {
                CacheManager.Instance.GetAssetsTransferred().Remove(asset);
            }
        }

        [TestMethod]
        public void CrossSiteUntransferredMediaRetainsSourceOriginAndIdentitiesWithWarning()
        {
            var path = SourcePath.Replace(".mp4", ".wav");
            using var source = CreateContext(SourceUrl, out var requests, filePath: path);
            using var target = CreateContext(TargetUrl, out var targetRequests);
            var observer = new Mock<ILogObserver>();
            AssertFile(Transform(source, target, new Dictionary<string, string> { ["MediaSource"] = path },
                requests, observer: observer.Object), SourceUrl, path, SourceId);
            Assert.IsFalse(targetRequests.Any(request => request.Contains("GetFile",StringComparison.OrdinalIgnoreCase)));
            observer.Verify(log => log.Warning(It.Is<LogEntry>(entry =>
                entry.Message.Contains("retaining its source location and identities"))), Times.Once);
        }

        [DataTestMethod]
        [DataRow("", "")]
        [DataRow("/_layouts/15/clientbin/mediaplaceholder.mp4", "")]
        [DataRow("https://external.example/clip.mp4", "")]
        [DataRow(SourcePath, "<iframe src=\"jav&#x61;script:alert(1)\"></iframe>")]
        [DataRow(SourcePath, "<iframe src=\"https://example.org\" onload=\"alert(1)\"></iframe>")]
        [DataRow(SourcePath, "<iframe srcdoc=\"&lt;script&gt;alert(1)&lt;/script&gt;\"></iframe>")]
        [DataRow(SourcePath, "<iframe src=\"https://example.org\"></iframe><script>alert(1)</script>")]
        public void InvalidConfigurationIsDiagnosedWithoutFileLookup(string media, string embed)
        {
            using var context = CreateContext(SourceUrl, out var requests);
            var observer = new Mock<ILogObserver>();
            AssertSkipped(() => Transform(context, context,
                new Dictionary<string, string> { ["MediaSource"] = media, ["VideoSetEmbedCode"] = embed }, requests,
                observer: observer.Object));
            observer.Verify(log => log.Warning(It.Is<LogEntry>(entry => entry.Message.Contains("skipped"))), Times.Once);
            Assert.IsFalse(requests.Any(request => request.Contains("GetFile",StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void MissingFileIsSkippedRatherThanEmittingEmptyIdentities()
        {
            using var context = CreateContext(SourceUrl, out var requests, exists: false);
            AssertSkipped(() => Transform(context, context, new Dictionary<string, string>
            {
                ["MediaSource"] = SourcePath
            }, requests));
        }

        private static void AssertSkipped(Action action)
        {
            var exception = Assert.ThrowsException<TargetInvocationException>(action);
            Assert.IsInstanceOfType(exception.InnerException, typeof(NotAvailableAtTargetException));
            StringAssert.Contains(exception.InnerException.Message, "skipped");
        }

        private static void AssertEmbed(JObject data)
        {
            Assert.AreEqual("1.2", (string)data["dataVersion"]);
            var properties = data["properties"];
            Assert.AreEqual("", (string)properties["cachedEmbedCode"]);
            Assert.AreEqual("", (string)properties["cachedEmbedCodeHtml"]);
            using (var document = new HtmlParser().ParseDocument((string)properties["embedCode"]))
            {
                var frame = document.QuerySelector("iframe");
                Assert.AreEqual(1, document.Body.Children.Length);
                Assert.AreEqual("https://example.org/embed?x=1&literal=&quot;&token=%7BTitle%7D", frame.GetAttribute("src"));
                Assert.AreEqual("640", frame.GetAttribute("width"));
                Assert.AreEqual("360", frame.GetAttribute("height"));
            }
        }

        private static void AssertFile(JObject data, string webUrl, string path, Guid identity)
        {
            var properties = data["properties"];
            Assert.AreEqual("1.4", (string)data["dataVersion"]);
            Assert.AreEqual(new Uri(webUrl).GetLeftPart(UriPartial.Authority) +
                string.Join("/", path.Split('/').Select(Uri.EscapeDataString)), (string)properties["file"]);
            Assert.AreEqual(path, (string)properties["serverRelativeUrl"]);
            Assert.AreEqual(path, (string)data["serverProcessedContent"]["links"]["serverRelativeUrl"]);
            Assert.AreEqual(webUrl, (string)properties["webAbsoluteUrl"]);
            foreach (var name in new[] { "siteId", "webId", "listId", "uniqueId" })
            {
                Assert.AreEqual(identity.ToString(), (string)properties[name], name);
            }
            Assert.AreEqual("Author \"name\" &quot; {Title}", (string)properties["authorName"]);
            Assert.AreEqual(path.Split('/').Last(), (string)data["serverProcessedContent"]["searchablePlainTexts"]["title"]);
        }

        private static JObject Transform(PnPContext source, PnPContext target, Dictionary<string, string> values,
            List<string> requests, IListItem page = null, ILogObserver observer = null)
        {
            var assembly = typeof(BuiltIn).Assembly;
            var resource = assembly.GetManifestResourceNames().Single(name => name.EndsWith("Modernization.webpartmapping.xml", StringComparison.Ordinal));
            PageTransformation transformation;
            using (var stream = assembly.GetManifestResourceStream(resource))
            {
                transformation = (PageTransformation)new XmlSerializer(typeof(PageTransformation)).Deserialize(stream);
            }
            var mapping = transformation.WebParts.Single(part => part.Type.StartsWith("Microsoft.SharePoint.Publishing.WebControls.MediaWebPart,", StringComparison.Ordinal));
            var part = new WebPartEntity { Type = mapping.Type };
            foreach (var property in mapping.Properties)
            {
                part.Properties[property.Name] = values.TryGetValue(property.Name, out var value) ? value : "";
            }
            part.Properties["Title"] = "Must not replace";
            var processor = new FunctionProcessor(source, target, null, transformation,
                new PageTransformationInformation(page) { SkipTelemetry = true }, observer == null ? null : new[] { observer });
            var before = requests.Count;
            var selected = processor.Process(ref mapping, part);
            Assert.IsFalse(requests.Skip(before).Any(request => request.Contains("GetFile",StringComparison.OrdinalIgnoreCase)|| request.Contains("EnsureSiteAssets",StringComparison.OrdinalIgnoreCase)));
            var branch = mapping.Mappings.Mapping.Single(item => item.Name == selected);
            processor.ProcessMappingFunctions(ref mapping, part, branch);
            Assert.AreEqual(selected == "EMBED" ? ClientSideWebPartType.ContentEmbed : ClientSideWebPartType.DocumentEmbed,
                branch.ClientSideWebPart.Single().Type);
            return JObject.Parse(WebUtility.HtmlDecode(TokenParser.ReplaceTokens(branch.ClientSideWebPart.Single().JsonControlData, part)));
        }

        private static PnPContext CreateContext(string url, out List<string> requests,
            Dictionary<string, object> fields = null, bool exists = true, string filePath = null)
        {
            CacheManager.Instance.SetSharePointVersion(new Uri(url), SPVersion.SPO);
            var identity = url == SourceUrl ? SourceId : TargetId;
            filePath ??= url == SourceUrl ? SourcePath : TargetPath;
            var file = new
            {
                Exists = exists, Name = filePath.Split('/').Last(), ServerRelativeUrl = filePath,
                ListId = identity, UniqueId = identity,
                Author = new { Id = 1, Title = "Author \"name\" &quot; {Title}" }
            };
            var library = new
            {
                Id = TargetId, Title = "Site Assets",
                RootFolder = new { UniqueId = TargetId, ServerRelativeUrl = "/sites/media-target/SiteAssets" }
            };
            var folders = new[]
            {
                new { UniqueId = SourceId, Name = "SitePages", ServerRelativeUrl = "/sites/media-target/SiteAssets/SitePages" },
                new { UniqueId = TargetId, Name = "clip", ServerRelativeUrl = TargetFolder }
            };
            var recorded = new List<string>();
            requests = recorded;
            return CoreTestContext.Create(url, (request, body) =>
            {
                var path = request.RequestUri.AbsolutePath;
                recorded.Add(request.RequestUri + " " + body);
                if (path.Contains("GetWebUrlFromPageUrl", StringComparison.OrdinalIgnoreCase))
                    return CoreTestContext.Json(new { value = url });
                if (path.EndsWith("/listitemallfields", StringComparison.OrdinalIgnoreCase))
                    return CoreTestContext.Json(fields);
                if (path.Contains("GetFileBy", StringComparison.OrdinalIgnoreCase))
                    return exists ? CoreTestContext.Json(file) :
                        CoreTestContext.Error("System.IO.FileNotFoundException", "File Not Found.", -2147024894);
                if (path.EndsWith("/folders", StringComparison.OrdinalIgnoreCase))
                    return CoreTestContext.Json(new { value = folders });
                if (path.Contains("GetFolderBy", StringComparison.OrdinalIgnoreCase))
                {
                    var address = Uri.UnescapeDataString(request.RequestUri.AbsoluteUri);
                    var existing = folders.OrderByDescending(folder => folder.ServerRelativeUrl.Length)
                        .FirstOrDefault(folder => address.Contains(folder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase));
                    if (existing != null) return CoreTestContext.Json(existing);
                    return CoreTestContext.Json(new
                    {
                        UniqueId = identity, Name = "video-set", ServerRelativeUrl = new Uri(url).AbsolutePath + "/Assets/video-set",
                        ListItemAllFields = fields
                    });
                }
                if (path.EndsWith("/lists", StringComparison.OrdinalIgnoreCase))
                    return CoreTestContext.Json(new { value = new[] { library } });
                if (path.Contains("/lists", StringComparison.OrdinalIgnoreCase))
                    return CoreTestContext.Json(library);
                throw new InvalidOperationException($"Unexpected media fixture request: {request.Method} {request.RequestUri}");
            }, new Dictionary<string, object> { ["Id"] = identity }, new Dictionary<string, object> { ["Id"] = identity });
        }
    }
}
