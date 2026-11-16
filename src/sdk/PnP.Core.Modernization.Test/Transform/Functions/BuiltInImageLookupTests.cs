using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Functions;
using PnP.Core.Modernization.Transform;
using PnP.Framework.Utilities.UnitTests.Model;
using PnP.Framework.Utilities.UnitTests.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace PnP.Core.Modernization.Test.Transform.Functions
{
    [TestClass]
    [TestCategory("Offline")]
    public class BuiltInImageLookupTests
    {
        private const string SiteUrl = "https://contoso.sharepoint.com/sites/modernization";
        private const string WebServerRelativeUrl = "/sites/modernization";
        private const string ImageServerRelativeUrl = "/sites/modernization/SiteAssets/green.svg";
        private static readonly Guid ImageListId = Guid.Parse("f266cd97-f155-4360-b802-47bb9f0d9b55");
        private static readonly Guid ImageUniqueId = Guid.Parse("a9b5b53e-2014-47d9-b26c-fc8d52047d1e");

        [TestMethod]
        public void ImageLookup_WhenBothDimensionsAreMissing_ReturnsNumericDefaults()
        {
            var result = InvokeImageLookup(false, null, false, null);

            AssertDimensions(result, "-1", "-1");
        }

        [TestMethod]
        public void ImageLookup_WhenOnlyWidthIsMissing_ReturnsWidthDefault()
        {
            var result = InvokeImageLookup(false, null, true, "360");

            AssertDimensions(result, "-1", "360");
        }

        [TestMethod]
        public void ImageLookup_WhenOnlyHeightIsMissing_ReturnsHeightDefault()
        {
            var result = InvokeImageLookup(true, "960", false, null);

            AssertDimensions(result, "960", "-1");
        }

        [TestMethod]
        public void ImageLookup_WhenDimensionsAreEmptyOrZero_ReturnsNumericDefaults()
        {
            var result = InvokeImageLookup(true, string.Empty, true, "0");

            AssertDimensions(result, "-1", "-1");
        }

        [TestMethod]
        public void ImageLookup_WhenDimensionsAreNull_ReturnsNumericDefaults()
        {
            var result = InvokeImageLookup(true, null, true, null);

            AssertDimensions(result, "-1", "-1");
        }

        [TestMethod]
        public void ImageLookup_WhenDimensionsAreValid_ReturnsPropertyValues()
        {
            var result = InvokeImageLookup(true, "960", true, 360);

            AssertDimensions(result, "960", "360");
        }

        [TestMethod]
        public void ImageLookup_WhenLookupIsSkipped_PreservesExistingDefaults()
        {
            var provider = CreateProvider(false, null, false, null);
            using (var context = CreateContext(provider))
            {
                var result = CreateBuiltIn(context).ImageLookup(null);

                Assert.IsNotNull(result);
                Assert.AreEqual("-1", result["ImageWidth"]);
                Assert.AreEqual("-1", result["ImageHeight"]);
                Assert.AreEqual(string.Empty, result["ImageListId"]);
                Assert.AreEqual(string.Empty, result["ImageUniqueId"]);
            }
        }

        [TestMethod]
        public void ImageLookup_WhenFileIsNotFound_ReturnsNumericDefaults()
        {
            using (var context = CreateContext(CreateProvider(false, null, false, null)))
            {
                var builtIn = CreateBuiltIn(context);
                context.Load(context.Web, web => web.ServerRelativeUrl);
                context.ExecuteQuery();
                context.WebRequestExecutorFactory = new MockWebRequestExecutorFactory(new StaticResponseProvider(
                    "[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.0.0\",\"ErrorInfo\":{\"ErrorMessage\":\"File Not Found.\",\"ErrorValue\":null,\"ErrorCode\":-2147024894,\"ErrorTypeName\":\"System.IO.FileNotFoundException\"},\"TraceCorrelationId\":\"d991e178-2b53-4ff8-b77f-6bb73516962a\"}]"));

                var result = builtIn.ImageLookup(ImageServerRelativeUrl);

                Assert.IsNotNull(result);
                Assert.AreEqual(string.Empty, result["ImageListId"]);
                Assert.AreEqual(string.Empty, result["ImageUniqueId"]);
                Assert.AreEqual("-1", result["ImageWidth"]);
                Assert.AreEqual("-1", result["ImageHeight"]);
                AssertMappingWithLookupResult("SharePointPnP.Modernization.WikiImagePart", result);
                AssertMappingWithLookupResult("Microsoft.SharePoint.WebPartPages.ImageWebPart, Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", result);
            }
        }

        [TestMethod]
        public void WikiImageMapping_WithDefaultDimensions_ProducesNumericJsonProperties()
        {
            AssertMappingWithDefaultDimensions("SharePointPnP.Modernization.WikiImagePart");
        }

        [TestMethod]
        public void ClassicImageWebPartMapping_WithDefaultDimensions_ProducesNumericJsonProperties()
        {
            AssertMappingWithDefaultDimensions("Microsoft.SharePoint.WebPartPages.ImageWebPart, Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");
        }

        private static void AssertMappingWithDefaultDimensions(string webPartType)
        {
            var lookupResult = InvokeImageLookup(false, null, false, null);
            AssertMappingWithLookupResult(webPartType, lookupResult);
        }

        [DataTestMethod]
        [DataRow("Image \"quote\" & ampersand", "Caption \"quote\" & detail", "https://contoso.sharepoint.com/?value=\"quote\"&next=1")]
        [DataRow("Path\\image\r\nnext\tline", "Caption\\detail\nnext", "https://contoso.sharepoint.com/")]
        [DataRow("Literal &quot; entity", "<caption> &amp; entity", "https://contoso.sharepoint.com/?value=&quot;")]
        [DataRow(null, null, "")]
        public void WikiImageMappingEscapesTextWithoutChangingJsonValues(string altText, string caption, string anchor)
        {
            using (var context = CreateContext(CreateProvider(false, null, false, null)))
            {
                var model = BasePageTransformator.LoadDefaultWebPartMapping();
                var mapping = model.WebParts.Single(part => part.Type == "SharePointPnP.Modernization.WikiImagePart");
                var data = new PnP.Core.Modernization.Entities.WebPartEntity
                {
                    Properties = new Dictionary<string, string>
                    {
                        ["ImageUrl"] = ImageServerRelativeUrl,
                        ["AlternativeText"] = altText,
                        ["Caption"] = caption,
                        ["Anchor"] = anchor,
                        ["SiteId"] = Guid.Empty.ToString(),
                        ["WebId"] = Guid.Empty.ToString()
                    }
                };
                var processor = new FunctionProcessor(context, context, null, model,
                    new PageTransformationInformation(null) { SkipTelemetry = true });

                processor.Process(ref mapping, data);
                var json = System.Net.WebUtility.HtmlDecode(TokenParser.ReplaceTokens(
                    mapping.Mappings.Mapping.Single().ClientSideWebPart.Single().JsonControlData, data));

                using (var document = JsonDocument.Parse(json))
                {
                    Assert.AreEqual(altText ?? "", document.RootElement.GetProperty("properties").GetProperty("altText").GetString());
                    var content = document.RootElement.GetProperty("serverProcessedContent");
                    Assert.AreEqual(caption ?? "", content.GetProperty("searchablePlainTexts").GetProperty("captionText").GetString());
                    Assert.AreEqual(anchor, content.GetProperty("links").GetProperty("linkUrl").GetString());
                }
            }
        }

        private static void AssertMappingWithLookupResult(string webPartType, Dictionary<string, string> lookupResult)
        {
            var mapping = LoadImageJsonControlData(webPartType);
            var replacements = new Dictionary<string, string>
            {
                ["{Caption}"] = string.Empty,
                ["{ServerRelativeFileName}"] = ImageServerRelativeUrl,
                ["{serverRelativeFileName}"] = ImageServerRelativeUrl,
                ["{Anchor}"] = string.Empty,
                ["{AlternativeText}"] = "Green image",
                ["{FileName}"] = "green.svg",
                ["{SiteId}"] = "c6815669-f27c-4ff6-b937-0825b2cdfa93",
                ["{webId}"] = "9d03e0aa-cb94-4463-b885-15a55d03ed52",
                ["{WebId}"] = "9d03e0aa-cb94-4463-b885-15a55d03ed52",
                ["{ImageListId}"] = lookupResult["ImageListId"],
                ["{ImageUniqueId}"] = lookupResult["ImageUniqueId"],
                ["{ImageWidth}"] = lookupResult["ImageWidth"],
                ["{ImageHeight}"] = lookupResult["ImageHeight"]
            };

            foreach (var replacement in replacements)
            {
                mapping = mapping.Replace(replacement.Key, replacement.Value);
            }

            using (var document = JsonDocument.Parse(mapping))
            {
                var properties = document.RootElement.GetProperty("properties");
                Assert.AreEqual(JsonValueKind.Number, properties.GetProperty("imgWidth").ValueKind);
                Assert.AreEqual(JsonValueKind.Number, properties.GetProperty("imgHeight").ValueKind);
                Assert.AreEqual(-1, properties.GetProperty("imgWidth").GetInt32());
                Assert.AreEqual(-1, properties.GetProperty("imgHeight").GetInt32());
            }
        }

        private static Dictionary<string, string> InvokeImageLookup(bool includeWidth, object width, bool includeHeight, object height)
        {
            using (var context = CreateContext(CreateProvider(includeWidth, width, includeHeight, height)))
            {
                return CreateBuiltIn(context).ImageLookup(ImageServerRelativeUrl);
            }
        }

        private static ClientContext CreateContext(IMockResponseProvider provider)
        {
            CacheManager.Instance.SetSharePointVersion(new Uri(SiteUrl), SPVersion.SPO);
            var context = new ClientContext(SiteUrl);
            context.WebRequestExecutorFactory = new MockWebRequestExecutorFactory(provider);
            return context;
        }

        private static BuiltIn CreateBuiltIn(ClientContext context)
        {
            return new BuiltIn(new PageTransformationInformation(null)
            {
                SkipTelemetry = true
            }, context);
        }

        private static MockEntryResponseProvider CreateProvider(bool includeWidth, object width, bool includeHeight, object height)
        {
            var properties = new Dictionary<string, object>
            {
                ["_ObjectType_"] = "SP.PropertyValues"
            };
            if (includeWidth)
            {
                properties["vti_lastwidth"] = width;
            }
            if (includeHeight)
            {
                properties["vti_lastheight"] = height;
            }

            var provider = new MockEntryResponseProvider();
            provider.ResponseEntries.Add(new MockResponseEntry<object>
            {
                Url = SiteUrl,
                PropertyName = "Site",
                ReturnValue = new Dictionary<string, object>
                {
                    ["_ObjectType_"] = "SP.Site",
                    ["ServerRelativeUrl"] = WebServerRelativeUrl,
                    ["Url"] = SiteUrl
                }
            });
            provider.ResponseEntries.Add(new MockResponseEntry<object>
            {
                Url = SiteUrl,
                PropertyName = "Web",
                ReturnValue = new Dictionary<string, object>
                {
                    ["_ObjectType_"] = "SP.Web",
                    ["ServerRelativeUrl"] = WebServerRelativeUrl,
                    ["Url"] = SiteUrl
                }
            });
            provider.ResponseEntries.Add(new MockResponseEntry<object>
            {
                Url = SiteUrl,
                Method = "GetFileByServerRelativeUrl",
                ReturnValue = new Dictionary<string, object>
                {
                    ["_ObjectType_"] = "SP.File",
                    ["ListId"] = $"/Guid({ImageListId})/",
                    ["UniqueId"] = $"/Guid({ImageUniqueId})/",
                    ["Properties"] = properties
                }
            });
            return provider;
        }

        private static string LoadImageJsonControlData(string webPartType)
        {
            var assembly = typeof(BuiltIn).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .Single(name => name.EndsWith("Modernization.webpartmapping.xml", StringComparison.Ordinal));
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                var document = XDocument.Load(stream);
                var schemaNamespace = document.Root.Name.Namespace;
                return document.Descendants(schemaNamespace + "WebPart")
                    .Single(element => (string)element.Attribute("Type") == webPartType)
                    .Descendants(schemaNamespace + "ClientSideWebPart")
                    .Single()
                    .Attribute("JsonControlData")
                    .Value;
            }
        }

        private static void AssertDimensions(Dictionary<string, string> result, string expectedWidth, string expectedHeight)
        {
            Assert.IsNotNull(result);
            Assert.AreEqual(ImageListId.ToString(), result["ImageListId"]);
            Assert.AreEqual(ImageUniqueId.ToString(), result["ImageUniqueId"]);
            Assert.AreEqual(expectedWidth, result["ImageWidth"]);
            Assert.AreEqual(expectedHeight, result["ImageHeight"]);
        }

        private sealed class StaticResponseProvider : IMockResponseProvider
        {
            private readonly string response;

            public StaticResponseProvider(string response)
            {
                this.response = response;
            }

            public string GetResponse(string url, string verb, string body)
            {
                return response;
            }
        }
    }
}
