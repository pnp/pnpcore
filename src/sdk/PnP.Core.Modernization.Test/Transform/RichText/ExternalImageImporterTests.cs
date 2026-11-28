using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Web;

namespace PnP.Core.Modernization.Test.Transform.RichText
{
    [TestClass]
    [TestCategory("Offline")]
    public class ExternalImageImporterTests
    {
        private static readonly Uri WebUrl = new Uri("https://contoso.sharepoint.com/sites/target");
        private static readonly Uri ExternalUrl = new Uri("https://images.example/photo?version=1&signature=test");

        [TestMethod]
        public void Import_ReusesAssetAcrossReferencesAndNewImporterInstances()
        {
            var store = new AssetStore { Folder = "/sites/target/SiteAssets/SitePages/server-selected-folder-17" };
            var firstRun = CreateImporter(store, "nested/renamed page.aspx");
            var first = firstRun.Import(ExternalUrl);
            var repeatedReference = firstRun.Import(ExternalUrl);
            var nextRun = CreateImporter(store, "nested/renamed page.aspx").Import(ExternalUrl);

            Assert.AreEqual(first, repeatedReference);
            Assert.AreEqual(first, nextRun);
            Assert.AreEqual(1, store.Imports);
            Assert.AreEqual(1, store.Files.Count);
            Assert.AreEqual(store.Files.Single().Value, first);
            Assert.IsTrue(first.StartsWith(store.Folder + "/", StringComparison.Ordinal));
            Assert.AreEqual("nested/renamed page.aspx", store.RequestedPageName);
            Assert.AreEqual("91", store.RequestedPageId);
        }

        [TestMethod]
        public void Import_PreservesQueryParametersAndEscapesODataStrings()
        {
            var source = new Uri("https://images.example/it's%20an%20image?size=200&signature=a%2Bb#view");
            var store = new AssetStore();
            CreateImporter(store, "nested/it's 100%.aspx").Import(source);

            Assert.AreEqual(source.AbsoluteUri, store.RequestedExternalUrl);
            Assert.AreEqual("nested/it's 100%.aspx", store.RequestedPageName);
            Assert.AreEqual(ExternalImageImporter.GetImageKey(source), store.RequestedFileName);
            Assert.IsFalse(store.RequestedFileName.Contains("signature"));
        }

        [TestMethod]
        public void Import_KeepsDifferentQueryStringsAsSeparateAssets()
        {
            var store = new AssetStore();
            var importer = CreateImporter(store);
            var first = importer.Import(ExternalUrl);
            var second = importer.Import(new Uri("https://images.example/photo?version=2&signature=test"));

            Assert.AreNotEqual(first, second);
            Assert.AreEqual(2, store.Files.Count);
            Assert.AreEqual(first, CreateImporter(store).Import(ExternalUrl));
            Assert.AreEqual(2, store.Imports);
        }

        [TestMethod]
        public void Import_FailureIsRetryableAndNeverCreatesASuccessfulMapping()
        {
            var store = new AssetStore { FailImport = true };
            Assert.ThrowsException<IOException>(() => CreateImporter(store).Import(ExternalUrl));
            Assert.AreEqual(0, store.Files.Count);

            store.FailImport = false;
            var imported = CreateImporter(store).Import(ExternalUrl);
            Assert.IsTrue(imported.StartsWith(store.Folder + "/", StringComparison.Ordinal));
            Assert.AreEqual(2, store.Imports);
            Assert.AreEqual(1, store.Files.Count);
        }

        [TestMethod]
        public void Import_FailedLookupDoesNotBlindlyDownloadAnotherCopy()
        {
            var store = new AssetStore();
            CreateImporter(store).Import(ExternalUrl);
            store.FailFileLookup = true;

            Assert.ThrowsException<IOException>(() => CreateImporter(store).Import(ExternalUrl));
            Assert.AreEqual(1, store.Imports);
            Assert.AreEqual(1, store.Files.Count);
        }

        [DataTestMethod]
        [DataRow("http://images.example/photo.png")]
        [DataRow("https://user:password@images.example/photo.png")]
        public void Import_RejectsUnsupportedUrlsBeforeAnyRequest(string url)
        {
            var importer = new ExternalImageImporter(WebUrl, 91, "page.aspx",
                (_, __) => throw new AssertFailedException("Unexpected request"));
            Assert.ThrowsException<InvalidDataException>(() => importer.Import(new Uri(url)));
        }

        [TestMethod]
        public void Import_RequiresASavedPageBeforeAnyRequest()
        {
            var importer = new ExternalImageImporter(WebUrl, 0, "page.aspx",
                (_, __) => throw new AssertFailedException("Unexpected request"));
            Assert.ThrowsException<InvalidDataException>(() => importer.Import(ExternalUrl));
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("not json")]
        [DataRow("{}")]
        public void Import_RejectsInvalidFolderResponses(string response)
        {
            var importer = new ExternalImageImporter(WebUrl, 91, "page.aspx", (_, __) => response);
            Assert.ThrowsException<InvalidDataException>(() => importer.Import(ExternalUrl));
        }

        private static ExternalImageImporter CreateImporter(AssetStore store, string pageName = "page.aspx")
        {
            return new ExternalImageImporter(WebUrl, 91, pageName, store.Request);
        }

        private sealed class AssetStore
        {
            internal string Folder = "/sites/target/SiteAssets/SitePages/server-owned-page";
            internal bool FolderExists;
            internal bool FailImport;
            internal bool FailFileLookup;
            internal int Imports;
            internal string RequestedPageName;
            internal string RequestedPageId;
            internal string RequestedExternalUrl;
            internal string RequestedFileName;
            internal readonly Dictionary<string, string> Files = new Dictionary<string, string>();

            internal string Request(HttpMethod method, string request)
            {
                var query = HttpUtility.ParseQueryString(new Uri(request).Query);
                string Unquote(string value) => value?.Trim('\'').Replace("''", "'");
                if (request.Contains("/GetOrCreateAssetFolder?"))
                {
                    Assert.AreEqual(HttpMethod.Post, method);
                    Assert.AreEqual("false", query["createFolderIfNeeded"]);
                    RequestedPageName = Unquote(query["pageName"]);
                    RequestedPageId = query["pageId"];
                    return JsonSerializer.Serialize(new { value = FolderExists ? Folder : "" });
                }
                if (request.Contains("/AddImageFromExternalUrl?"))
                {
                    Assert.AreEqual(HttpMethod.Post, method);
                    RequestedPageName = Unquote(query["pageName"]);
                    RequestedExternalUrl = Unquote(query["externalUrl"]);
                    RequestedFileName = Unquote(query["imageFileName"]);
                    Imports++;
                    if (FailImport) throw new IOException("External download failed");
                    FolderExists = true;
                    var name = RequestedFileName + ".png";
                    Files[name] = Folder + "/" + name;
                    return JsonSerializer.Serialize(new { ServerRelativeUrl = Files[name] });
                }
                Assert.AreEqual(HttpMethod.Get, method);
                Assert.IsTrue(request.Contains("/GetFolderByServerRelativePath("));
                Assert.AreEqual("2", query["$top"]);
                if (FailFileLookup) throw new IOException("Asset listing unavailable");
                var key = query["$filter"].Split('\'')[1];
                return JsonSerializer.Serialize(new
                {
                    value = Files.Where(file => file.Key == key || file.Key.StartsWith(key + ".", StringComparison.Ordinal))
                        .Select(file => new { Name = file.Key, ServerRelativeUrl = file.Value }).Take(2).ToArray()
                });
            }
        }
    }
}
