using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace PnP.Core.Provisioning.Test.Offline.ObjectHandlers
{
    /// <summary>
    /// Pins the JSON bodies <c>ObjectTenant</c> sends to SharePoint.
    /// </summary>
    [TestClass]
    [TestCategory("Offline")]
    public class TenantPayloadTests
    {
        #region Themes

        [TestMethod]
        public void ThemeJson_HasTheThreeMembersSharePointReads()
        {
            string json = TenantThemes.BuildThemeJson(
                "Contoso", "{\"themePrimary\":\"#0078d4\",\"themeDark\":\"#005a9e\"}", isInverted: false);

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement root = document.RootElement;

                Assert.AreEqual("Contoso", root.GetProperty("name").GetString());
                Assert.IsFalse(root.GetProperty("isInverted").GetBoolean());
                Assert.AreEqual("#0078d4", root.GetProperty("palette").GetProperty("themePrimary").GetString());
                Assert.AreEqual("#005a9e", root.GetProperty("palette").GetProperty("themeDark").GetString());
            }
        }

        [TestMethod]
        public void ThemeJson_CarriesTheInvertedFlag()
        {
            string json = TenantThemes.BuildThemeJson("Dark", "{\"themePrimary\":\"#fff\"}", isInverted: true);

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                Assert.IsTrue(document.RootElement.GetProperty("isInverted").GetBoolean());
            }
        }

        [TestMethod]
        public void ThemeJson_PassesThePaletteThroughUntouched()
        {
            string json = TenantThemes.BuildThemeJson("Odd", "{\"a\":1,\"b\":{\"c\":true}}", isInverted: false);

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement palette = document.RootElement.GetProperty("palette");

                Assert.AreEqual(1, palette.GetProperty("a").GetInt32());
                Assert.IsTrue(palette.GetProperty("b").GetProperty("c").GetBoolean());
            }
        }

        [TestMethod]
        public void ThemeJson_TreatsAnEmptyPaletteAsAnEmptyObject()
        {
            foreach (string empty in new[] { null, "", "   " })
            {
                using (JsonDocument document = JsonDocument.Parse(
                    TenantThemes.BuildThemeJson("Bare", empty, isInverted: false)))
                {
                    Assert.AreEqual(JsonValueKind.Object, document.RootElement.GetProperty("palette").ValueKind);
                }
            }
        }

        [TestMethod]
        public void ThemeJson_EscapesANameThatNeedsIt()
        {
            string json = TenantThemes.BuildThemeJson("He said \"hi\"", "{}", isInverted: false);

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                Assert.AreEqual("He said \"hi\"", document.RootElement.GetProperty("name").GetString());
            }
        }

        [TestMethod]
        public void ThemeJson_RefusesAPaletteThatIsNotAnObject()
        {
            Assert.ThrowsException<ArgumentException>(
                () => TenantThemes.BuildThemeJson("Bad", "[1,2,3]", isInverted: false));
        }

        #endregion

        #region Site designs

        [TestMethod]
        public void SiteDesignPayload_HasThePropertiesTheEndpointDefines()
        {
            var info = new SiteScriptUtility.SiteDesignInfo
            {
                Title = "Contoso project",
                Description = "For projects",
                PreviewImageUrl = "https://contoso.example/preview.png",
                PreviewImageAltText = "A preview",
                IsDefault = true,
                WebTemplate = "64",
            };

            info.SiteScriptIds.Add("07702c07-0dbe-4e8d-bad9-5d3a1b0e5c4f");

            Dictionary<string, object> payload = info.ToPayload();

            Assert.AreEqual("Contoso project", payload["Title"]);
            Assert.AreEqual("For projects", payload["Description"]);
            Assert.AreEqual("https://contoso.example/preview.png", payload["PreviewImageUrl"]);
            Assert.AreEqual("A preview", payload["PreviewImageAltText"]);
            Assert.AreEqual(true, payload["IsDefault"]);
            Assert.AreEqual("64", payload["WebTemplate"]);

            var scriptIds = (Dictionary<string, object>)payload["SiteScriptIds"];

            CollectionAssert.AreEqual(
                new[] { "07702c07-0dbe-4e8d-bad9-5d3a1b0e5c4f" },
                (System.Collections.ICollection)scriptIds["results"]);
        }

        [TestMethod]
        public void SiteDesignPayload_SerialisesWithoutLosingAnything()
        {
            var info = new SiteScriptUtility.SiteDesignInfo { Title = "T", WebTemplate = "68" };

            string json = JsonSerializer.Serialize(new Dictionary<string, object> { ["info"] = info.ToPayload() });

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement wrapped = document.RootElement.GetProperty("info");

                Assert.AreEqual("T", wrapped.GetProperty("Title").GetString());
                Assert.AreEqual("68", wrapped.GetProperty("WebTemplate").GetString());
                Assert.AreEqual(JsonValueKind.Array,
                    wrapped.GetProperty("SiteScriptIds").GetProperty("results").ValueKind);
            }
        }

        #endregion

        #region Verbose OData envelopes


        [TestMethod]
        public void Unwrap_DescendsThroughAMethodNameWrapper()
        {
            using (JsonDocument document = JsonDocument.Parse(
                "{\"d\":{\"GetSiteScripts\":{\"__metadata\":{\"type\":\"Collection(…)\"}," +
                "\"results\":[{\"Id\":\"016d6bd6-ebf8-4be9-8256-d3979abf4fe4\"}]}}}"))
            {
                JsonElement unwrapped = VerboseOData.Unwrap(document.RootElement);

                Assert.AreEqual(JsonValueKind.Array, unwrapped.ValueKind);
                Assert.AreEqual(1, unwrapped.GetArrayLength());
            }
        }

        [TestMethod]
        public void Unwrap_StopsWhenDHoldsTheObjectItself()
        {
            using (JsonDocument document = JsonDocument.Parse(
                "{\"d\":{\"__metadata\":{\"type\":\"Microsoft.SharePoint.ClientSideComponent.StorageEntity\"}," +
                "\"Comment\":\"probe\",\"Description\":\"probe\",\"Value\":\"provisioned\"}}"))
            {
                Assert.AreEqual("provisioned",
                    VerboseOData.StringOf(VerboseOData.Unwrap(document.RootElement), "Value"));
            }
        }

        [TestMethod]
        public void Unwrap_HandlesAMethodThatReturnsNothing()
        {
            using (JsonDocument document = JsonDocument.Parse("{\"d\":{\"SetStorageEntity\":null}}"))
            {
                Assert.AreEqual(JsonValueKind.Null, VerboseOData.Unwrap(document.RootElement).ValueKind);
            }
        }

        [TestMethod]
        public void Unwrap_LeavesAnAlreadyUnwrappedResponseAlone()
        {
            using (JsonDocument document = JsonDocument.Parse("{\"Value\":\"plain\"}"))
            {
                Assert.AreEqual("plain",
                    VerboseOData.StringOf(VerboseOData.Unwrap(document.RootElement), "Value"));
            }
        }

        [TestMethod]
        public void CollectionOf_ReadsTheResultsWrapper()
        {
            using (JsonDocument document = JsonDocument.Parse(
                "{\"SiteScriptIds\":{\"__metadata\":{\"type\":\"Collection(Edm.Guid)\"}," +
                "\"results\":[\"856691ed-2e74-4dac-b92d-aac8fdb1a902\"]}}"))
            {
                var ids = new List<string>();

                foreach (JsonElement id in VerboseOData.CollectionOf(document.RootElement, "SiteScriptIds"))
                {
                    ids.Add(id.GetString());
                }

                CollectionAssert.AreEqual(new[] { "856691ed-2e74-4dac-b92d-aac8fdb1a902" }, ids);
            }
        }

        [TestMethod]
        public void CollectionOf_AlsoReadsABareArray()
        {
            using (JsonDocument document = JsonDocument.Parse("{\"Ids\":[\"a\",\"b\"]}"))
            {
                var ids = new List<string>();

                foreach (JsonElement id in VerboseOData.CollectionOf(document.RootElement, "Ids"))
                {
                    ids.Add(id.GetString());
                }

                CollectionAssert.AreEqual(new[] { "a", "b" }, ids);
            }
        }

        [TestMethod]
        public void CollectionOf_IsEmptyForAnAbsentProperty()
        {
            using (JsonDocument document = JsonDocument.Parse("{}"))
            {
                Assert.AreEqual(0, VerboseOData.CollectionOf(document.RootElement, "Missing").Count());
            }
        }

        #endregion

        #region CDN policies

        [TestMethod]
        public void CdnPolicies_ReadsTheNameValuePairsSharePointReturns()
        {
            Dictionary<TenantCdnPolicyType, string> parsed = TenantCdnPolicies.Parse(new[]
            {
                "IncludeFileExtensions;CSS,EOT,GIF,ICO,JPEG,JPG,JS,MAP,PNG,SVG,TTF,WOFF",
                "ExcludeRestrictedSiteClassifications;",
                "ExcludeIfNoScriptDisabled;False",
            });

            Assert.AreEqual(3, parsed.Count);
            Assert.AreEqual("CSS,EOT,GIF,ICO,JPEG,JPG,JS,MAP,PNG,SVG,TTF,WOFF",
                parsed[TenantCdnPolicyType.IncludeFileExtensions]);
            Assert.AreEqual(string.Empty, parsed[TenantCdnPolicyType.ExcludeRestrictedSiteClassifications]);
            Assert.AreEqual("False", parsed[TenantCdnPolicyType.ExcludeIfNoScriptDisabled]);
        }

        [TestMethod]
        public void CdnPolicies_KeepEverythingAfterTheFirstSeparator()
        {
            Dictionary<TenantCdnPolicyType, string> parsed = TenantCdnPolicies.Parse(
                new[] { "IncludeFileExtensions;a;b;c" });

            Assert.AreEqual("a;b;c", parsed[TenantCdnPolicyType.IncludeFileExtensions]);
        }

        [TestMethod]
        public void CdnPolicies_SkipAnUnknownPolicyRatherThanFailing()
        {
            Dictionary<TenantCdnPolicyType, string> parsed = TenantCdnPolicies.Parse(new[]
            {
                "SomePolicyAddedNextYear;whatever",
                "IncludeFileExtensions;CSS",
            });

            Assert.AreEqual(1, parsed.Count);
            Assert.AreEqual("CSS", parsed[TenantCdnPolicyType.IncludeFileExtensions]);
        }

        [TestMethod]
        public void CdnPolicies_SurviveMalformedEntries()
        {
            Dictionary<TenantCdnPolicyType, string> parsed = TenantCdnPolicies.Parse(
                new[] { null, "", "no-separator", "IncludeFileExtensions;CSS" });

            Assert.AreEqual(1, parsed.Count);
        }

        [TestMethod]
        public void CdnPolicies_AreEmptyForNoEntries()
        {
            Assert.AreEqual(0, TenantCdnPolicies.Parse(null).Count);
            Assert.AreEqual(0, TenantCdnPolicies.Parse(new string[0]).Count);
        }

        #endregion
    }
}
