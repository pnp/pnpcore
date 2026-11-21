using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core;
using PnP.Core.Modernization.Publishing;

using BuiltInContentTypeId = PnP.Core.Provisioning.BuiltInContentTypeId;

namespace PnP.Core.Modernization.Tests.Transform.Publishing
{
    [TestClass]
    [TestCategory("Offline")]
    public class EnterpriseWikiPagePreparationTests
    {
        private static readonly Guid SiteId = Guid.NewGuid();
        private static readonly Guid WebId = Guid.NewGuid();

        [DataTestMethod]
        [DataRow("ENTERWIKI")]
        [DataRow("ESWIKI")]
        public void OptedInEnterpriseWikiDestinationPreparesOnce(string template)
        {
            var preparation = new EnterpriseWikiPagePreparation();
            var calls = 0;
            for (var page = 0; page < 3; page++)
            {
                preparation.EnsureReady(SiteId, WebId, template, false, true, () => calls++);
            }
            Assert.AreEqual(1, calls);
        }

        [TestMethod]
        public void OrdinaryPageOnOrdinaryWebDoesNotPrepareEvenWithOptIn()
        {
            var calls = 0;
            new EnterpriseWikiPagePreparation().EnsureReady(SiteId, WebId, "STS", false, true, () => calls++);
            Assert.AreEqual(0, calls);
        }

        [DataTestMethod]
        [DataRow("STS")]
        [DataRow("ENTERWIKI")]
        [DataRow("ESWIKI")]
        public void OmittedFlagSkipsPreparationBeforeAndAfterOptIn(string template)
        {
            var preparation = new EnterpriseWikiPagePreparation();
            var calls = 0;
            var information = new PublishingPageTransformationInformation(null);
            preparation.EnsureReady(SiteId, WebId, template, true, information.EnableEnterpriseWikiCapabilities, () => calls++);
            Assert.AreEqual(0, calls);
            preparation.EnsureReady(SiteId, WebId, template, true, true, () => calls++);
            preparation.EnsureReady(SiteId, WebId, template, true, true, () => calls++);
            Assert.AreEqual(1, calls);
            preparation.EnsureReady(SiteId, WebId, template, true, false, () => calls++);
            Assert.AreEqual(1, calls);
        }

        [TestMethod]
        public void FailedPreparationCanBeRetried()
        {
            var preparation = new EnterpriseWikiPagePreparation();
            Assert.ThrowsException<InvalidOperationException>(() =>
                preparation.EnsureReady(SiteId, WebId, "ENTERWIKI", true, true,
                    () => throw new InvalidOperationException("Field provisioning failed.")));
            var calls = 0;
            preparation.EnsureReady(SiteId, WebId, "ENTERWIKI", true, true, () => calls++);
            Assert.AreEqual(1, calls);
        }

        [TestMethod]
        public void PreparationIsScopedToSiteWebAndTransformatorLifetime()
        {
            var preparation = new EnterpriseWikiPagePreparation();
            var calls = 0;
            preparation.EnsureReady(SiteId, WebId, "ESWIKI", true, true, () => calls++);
            preparation.EnsureReady(SiteId, Guid.NewGuid(), "ESWIKI", true, true, () => calls++);
            preparation.EnsureReady(Guid.NewGuid(), WebId, "ESWIKI", true, true, () => calls++);
            new EnterpriseWikiPagePreparation().EnsureReady(SiteId, WebId, "ESWIKI", true, true, () => calls++);
            Assert.AreEqual(4, calls);
        }

        [TestMethod]
        public void DetectsEnterpriseWikiPagesWithoutMatchingOrdinaryPages()
        {
            Assert.IsTrue(EnterpriseWikiPagePreparation.IsEnterpriseWikiPage(
                BuiltInContentTypeId.EnterpriseWikiPage.ToLowerInvariant() + "00" + Guid.NewGuid().ToString("N"), null));
            Assert.IsFalse(EnterpriseWikiPagePreparation.IsEnterpriseWikiPage(BuiltInContentTypeId.ModernArticlePage, null));
            Assert.IsTrue(EnterpriseWikiPagePreparation.IsEnterpriseWikiPage(null, "/_catalogs/masterpage/EnterpriseWiki.aspx"));
            Assert.IsFalse(EnterpriseWikiPagePreparation.IsEnterpriseWikiPage(null, "/_catalogs/masterpage/ArticleLeft.aspx"));
        }

        [TestMethod]
        public void RejectsSameNameDifferentIdOrDifferentNativeType()
        {
            var id = Guid.NewGuid();
            var expected = $"<Field ID='{id}' Name='AverageRating' Type='AverageRating'/>";
            Assert.ThrowsException<InvalidOperationException>(() =>
                EnterpriseWikiPagePreparation.ValidateFieldSchema(expected,
                    $"<Field ID='{Guid.NewGuid()}' Name='AverageRating' Type='AverageRating'/>"));
            Assert.ThrowsException<InvalidOperationException>(() =>
                EnterpriseWikiPagePreparation.ValidateFieldSchema(expected,
                    $"<Field ID='{id}' Name='AverageRating' Type='Number'/>"));
        }

        [DataTestMethod]
        [DataRow("SspId")]
        [DataRow("TermSetId")]
        [DataRow("TextField")]
        public void RejectsChangedOrMissingTaxonomyBindings(string property)
        {
            var schema = CategorySchema();
            var propertyXml = $"<Name>{property}</Name><Value>{SiteId}</Value>";
            Assert.ThrowsException<InvalidOperationException>(() =>
                EnterpriseWikiPagePreparation.ValidateFieldSchema(schema,
                    schema.Replace(propertyXml, $"<Name>{property}</Name><Value>{Guid.NewGuid()}</Value>")));
            Assert.ThrowsException<InvalidOperationException>(() =>
                EnterpriseWikiPagePreparation.ValidateFieldSchema(schema,
                    schema.Replace(propertyXml, $"<Name>{property}</Name><Value>{Guid.Empty}</Value>")));
        }

        [TestMethod]
        public void PreparedLibraryIsVerifiedWithoutServerWrites()
        {
            var fixture = new PreparationFixture();
            using (var context = fixture.CreateContext())
            {
                EnterpriseWikiPagePreparation.Prepare(context);
                EnterpriseWikiPagePreparation.Prepare(context);
            }
            Assert.IsTrue(fixture.RequestCount > 0);
            Assert.AreEqual(0, fixture.WriteCount);
        }

        [TestMethod]
        public void MissingRatingDefinitionFailsBeforeAnyServerWrite()
        {
            var fixture = new PreparationFixture();
            fixture.SiteFields.RemoveAll(f => f["InternalName"].ToString() == "Ratings");
            using (var context = fixture.CreateContext())
            {
                var exception = Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiPagePreparation.Prepare(context));
                StringAssert.Contains(exception.Message, "Ratings infrastructure");
            }
            Assert.AreEqual(0, fixture.WriteCount);
        }

        [TestMethod]
        public void ExistingConflictingColumnFailsBeforeAnyServerWrite()
        {
            var fixture = new PreparationFixture();
            var field = fixture.ListFields.First(f => f["InternalName"].ToString() == "AverageRating");
            field["SchemaXml"] = field["SchemaXml"].ToString().Replace("Type='AverageRating'", "Type='Number'");
            field["TypeAsString"] = "Number";
            using (var context = fixture.CreateContext())
            {
                var exception = Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiPagePreparation.Prepare(context));
                StringAssert.Contains(exception.Message, "Conflicting SitePages column");
            }
            Assert.AreEqual(0, fixture.WriteCount);
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void MissingSitePagesFeatureFailsBeforeAnyServerWrite(bool hasUnrelatedFeature)
        {
            var fixture = new PreparationFixture();
            fixture.Features.Clear();
            if (hasUnrelatedFeature)
            {
                fixture.Features.Add(new
                {
                    DefinitionId = new Guid("94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb")
                });
            }
            using (var context = fixture.CreateContext())
            {
                var exception = Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiPagePreparation.Prepare(context));
                StringAssert.Contains(exception.Message, "Site Pages feature is not enabled");
                StringAssert.Contains(exception.Message, context.Uri.AbsoluteUri);
                StringAssert.Contains(exception.Message, EnterpriseWikiPagePreparation.SitePagesFeatureId.ToString());
                StringAssert.Contains(exception.Message, "Manage site features");
            }
            Assert.AreEqual(1, fixture.FeatureQueryCount);
            Assert.AreEqual(0, fixture.WriteCount);
            Assert.AreEqual(hasUnrelatedFeature ? 1 : 0, fixture.Features.Count);
        }

        [TestMethod]
        public void MissingSitePagesFeatureIsRejectedBeforeColumnValidation()
        {
            var fixture = new PreparationFixture();
            fixture.Features.Clear();
            fixture.SiteFields.Clear();
            using (var context = fixture.CreateContext())
            {
                var exception = Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiPagePreparation.Prepare(context));
                StringAssert.Contains(exception.Message, "Site Pages feature is not enabled");
            }
            Assert.AreEqual(1, fixture.FeatureQueryCount);
            Assert.AreEqual(0, fixture.WriteCount);
        }

        [DataTestMethod]
        [DataRow("ENTERWIKI", false)]
        [DataRow("ESWIKI", false)]
        [DataRow("STS", true)]
        public void EligibleEnterpriseWikiDestinationsRequireAnEnabledFeature(string template, bool isEnterpriseWikiPage)
        {
            var fixture = new PreparationFixture();
            fixture.Features.Clear();
            using (var context = fixture.CreateContext())
            {
                var preparation = new EnterpriseWikiPagePreparation();
                var exception = Assert.ThrowsException<InvalidOperationException>(() =>
                    preparation.EnsureReady(SiteId, WebId, template, isEnterpriseWikiPage, true,
                        () => EnterpriseWikiPagePreparation.Prepare(context)));
                StringAssert.Contains(exception.Message, "Site Pages feature is not enabled");
            }
            Assert.AreEqual(1, fixture.FeatureQueryCount);
            Assert.AreEqual(0, fixture.WriteCount);
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void PreparationRefreshesFeatureStateOnAReusedContext(bool initiallyEnabled)
        {
            var fixture = new PreparationFixture();
            if (!initiallyEnabled)
            {
                fixture.Features.Clear();
            }
            using (var context = fixture.CreateContext())
            {
                if (initiallyEnabled)
                {
                    EnterpriseWikiPagePreparation.Prepare(context);
                    fixture.Features.Clear();
                    Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiPagePreparation.Prepare(context));
                }
                else
                {
                    Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiPagePreparation.Prepare(context));
                    fixture.Features.Add(PreparationFixture.FeatureResponse());
                    EnterpriseWikiPagePreparation.Prepare(context);
                }
            }
            Assert.AreEqual(0, fixture.WriteCount);
            Assert.AreEqual(initiallyEnabled ? 0 : 1, fixture.Features.Count);
        }

        [TestMethod]
        public void FeatureQueryFailureStopsPreparationWithoutServerWrites()
        {
            var fixture = new PreparationFixture { FailRequests = true };
            using (var context = fixture.CreateContext())
            {
                var exception = Assert.ThrowsException<SharePointRestServiceException>(() => EnterpriseWikiPagePreparation.Prepare(context));
                StringAssert.Contains(((SharePointRestError)exception.Error).Message, "Feature query failed");
                Assert.IsFalse(exception.Message.Contains("Site Pages feature is not enabled"));
            }
            Assert.AreEqual(1, fixture.FeatureQueryCount);
            Assert.AreEqual(0, fixture.WriteCount);
        }

        [TestMethod]
        public void MissingTaxonomyBackingColumnIsAddedFromItsSiteColumnOnce()
        {
            var fixture = new PreparationFixture();
            fixture.ListFields.RemoveAll(f => f["InternalName"].ToString() == "CategoryBacking");
            fixture.AddBackingField = true;
            using (var context = fixture.CreateContext())
            {
                EnterpriseWikiPagePreparation.Prepare(context);
                EnterpriseWikiPagePreparation.Prepare(context);
            }
            Assert.AreEqual(1, fixture.WriteCount);
            Assert.AreEqual(1, fixture.ListFields.Count(f => f["InternalName"].ToString() == "CategoryBacking"));
        }

        private sealed class PreparationFixture
        {
            private const string WebUrl = "https://contoso.sharepoint.com/sites/wiki";
            private readonly Guid libraryId = Guid.NewGuid();
            private readonly string contentTypeId = BuiltInContentTypeId.ModernArticlePage + "00" + Guid.NewGuid().ToString("N");
            private readonly List<Dictionary<string, object>> fieldLinks = new List<Dictionary<string, object>>();
            internal readonly List<Dictionary<string, object>> SiteFields = new List<Dictionary<string, object>>();
            internal readonly List<Dictionary<string, object>> ListFields = new List<Dictionary<string, object>>();
            internal readonly List<object> Features = new List<object>();
            internal bool FailRequests { get; set; }
            internal bool AddBackingField { get; set; }
            internal int RequestCount { get; private set; }
            internal int FeatureQueryCount { get; private set; }
            internal int WriteCount { get; private set; }

            internal PreparationFixture()
            {
                var fields = new[]
                {
                    (SiteId, "CategoryBacking", "Note", true),
                    (new Guid("4d64b067-08c3-43dc-a87b-8b8e01673313"), "RatedBy", "UserMulti", true),
                    (new Guid("434f51fb-ffd2-4a0e-a03b-ca3131ac67ba"), "Ratings", "Note", true),
                    (new Guid("b1996002-9167-45e5-a4df-b2c41c6723c7"), "RatingCount", "RatingCount", false),
                    (new Guid("5a14d1ab-1513-48c7-97b3-657a5ba6c742"), "AverageRating", "AverageRating", false),
                    (WebId, EnterpriseWikiPagePreparation.CategoryFieldName, "TaxonomyFieldTypeMulti", false)
                };
                foreach (var (id, name, type, hidden) in fields)
                {
                    var schema = id == WebId ? CategorySchema() : $"<Field ID='{id}' Name='{name}' Type='{type}'/>";
                    var element = XElement.Parse(schema);
                    element.SetAttributeValue("Hidden", hidden ? "TRUE" : "FALSE");
                    if (!hidden)
                    {
                        element.SetAttributeValue("ShowInEditForm", id == WebId ? "TRUE" : "FALSE");
                        element.SetAttributeValue("ShowInNewForm", id == WebId ? "TRUE" : "FALSE");
                    }
                    var field = new Dictionary<string, object>
                    {
                        ["odata.id"] = $"{WebUrl}/_api/Web/Fields(guid'{id}')",
                        ["Id"] = id,
                        ["Title"] = name,
                        ["InternalName"] = name,
                        ["TypeAsString"] = type,
                        ["SchemaXml"] = element.ToString().Replace('"', '\''),
                        ["Hidden"] = hidden,
                        ["ReadOnlyField"] = false
                    };
                    SiteFields.Add(field);
                    var listField = new Dictionary<string, object>(field)
                    {
                        ["odata.id"] = $"{WebUrl}/_api/Web/Lists(guid'{libraryId}')/Fields(guid'{id}')"
                    };
                    ListFields.Add(listField);
                    fieldLinks.Add(new Dictionary<string, object>
                    {
                        ["Id"] = id,
                        // A native backing field is hidden on the list, not necessarily on its link.
                        ["Hidden"] = id != SiteId && hidden,
                        ["ReadOnly"] = false,
                        ["Name"] = name
                    });
                }
                Features.Add(FeatureResponse());
            }

            internal static object FeatureResponse() => new { DefinitionId = EnterpriseWikiPagePreparation.SitePagesFeatureId };

            internal PnPContext CreateContext() => CoreTestContext.Create(WebUrl, Respond,
                webProperties: new Dictionary<string, object>
                {
                    ["Id"] = WebId,
                    ["Features"] = Features
                },
                siteProperties: new Dictionary<string, object> { ["Id"] = SiteId });

            private Dictionary<string, object> Library(string query)
            {
                var result = new Dictionary<string, object>
                {
                    ["Id"] = libraryId,
                    ["Title"] = "SitePages",
                    ["BaseTemplate"] = 119,
                    ["ListItemEntityTypeFullName"] = "SP.Data.SitePagesItem",
                    ["RootFolder"] = new { Name = "SitePages", ServerRelativeUrl = "/sites/wiki/SitePages" }
                };
                if (query.Contains("Fields", StringComparison.OrdinalIgnoreCase)) result["Fields"] = ListFields;
                if (query.Contains("ContentTypes", StringComparison.OrdinalIgnoreCase))
                    result["ContentTypes"] = new[] { ContentType(query) };
                return result;
            }

            private Dictionary<string, object> ContentType(string query)
            {
                var result = new Dictionary<string, object>
                {
                    ["StringId"] = contentTypeId,
                    ["Id"] = new { StringValue = contentTypeId },
                    ["Name"] = "Site Page"
                };
                if (query.Contains("FieldLinks", StringComparison.OrdinalIgnoreCase)) result["FieldLinks"] = FieldLinks(query);
                return result;
            }

            private Dictionary<string, object>[] FieldLinks(string query)
            {
                // REST omits ReadOnly unless it is explicitly selected.
                var selected = query.Contains("ReadOnly", StringComparison.Ordinal);
                return fieldLinks.Select(link =>
                {
                    var result = new Dictionary<string, object>(link);
                    if (!selected) result.Remove("ReadOnly");
                    return result;
                }).ToArray();
            }

            private HttpResponseMessage Respond(HttpRequestMessage request, string body)
            {
                RequestCount++;
                var path = Uri.UnescapeDataString(request.RequestUri.AbsolutePath);
                var query = Uri.UnescapeDataString(request.RequestUri.Query);
                if (request.Method != HttpMethod.Get)
                {
                    return Write(request, body, path);
                }

                if (path.EndsWith("/features", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith("/_api/web", StringComparison.OrdinalIgnoreCase) &&
                    query.Contains("Features", StringComparison.OrdinalIgnoreCase))
                {
                    FeatureQueryCount++;
                    StringAssert.Contains(query, "DefinitionId");
                    if (FailRequests)
                    {
                        return CoreTestContext.Error("System.UnauthorizedAccessException", "Feature query failed.", -2147024891);
                    }
                    if (path.EndsWith("/features", StringComparison.OrdinalIgnoreCase))
                        return CoreTestContext.Json(new { value = Features });
                }
                if (path.EndsWith("/_api/web", StringComparison.OrdinalIgnoreCase))
                {
                    var web = new Dictionary<string, object>
                    {
                        ["Id"] = WebId,
                        ["ServerRelativeUrl"] = "/sites/wiki"
                    };
                    if (query.Contains("Features", StringComparison.OrdinalIgnoreCase)) web["Features"] = Features;
                    if (query.Contains("AvailableFields", StringComparison.OrdinalIgnoreCase)) web["AvailableFields"] = SiteFields;
                    if (query.Contains("Lists", StringComparison.OrdinalIgnoreCase)) web["Lists"] = new[] { Library(query) };
                    return CoreTestContext.Json(web);
                }
                if (path.EndsWith("/availablefields", StringComparison.OrdinalIgnoreCase))
                {
                    return CoreTestContext.Json(new { value = SiteFields });
                }
                if (path.EndsWith("/effectivebasepermissions", StringComparison.OrdinalIgnoreCase))
                {
                    return CoreTestContext.Json(new { High = "2147483647", Low = "4294967295" });
                }
                if (path.EndsWith("/fieldlinks", StringComparison.OrdinalIgnoreCase))
                {
                    return CoreTestContext.Json(new { value = FieldLinks(query) });
                }
                if (path.EndsWith("/fields", StringComparison.OrdinalIgnoreCase))
                {
                    return CoreTestContext.Json(new
                    {
                        value = path.Contains("/lists(", StringComparison.OrdinalIgnoreCase) ||
                            path.Contains("/getlist(", StringComparison.OrdinalIgnoreCase) ? ListFields : SiteFields
                    });
                }
                if (path.EndsWith("/contenttypes", StringComparison.OrdinalIgnoreCase))
                {
                    return CoreTestContext.Json(new { value = new[] { ContentType(query) } });
                }
                if (path.Contains("/contenttypes(", StringComparison.OrdinalIgnoreCase))
                {
                    return CoreTestContext.Json(ContentType(query));
                }
                if (path.EndsWith("/lists", StringComparison.OrdinalIgnoreCase))
                {
                    return CoreTestContext.Json(new { value = new[] { Library(query) } });
                }
                if (path.Contains("/lists(", StringComparison.OrdinalIgnoreCase) ||
                    path.Contains("/getlist(", StringComparison.OrdinalIgnoreCase))
                {
                    return CoreTestContext.Json(Library(query));
                }
                throw new InvalidOperationException($"Unexpected preparation fixture request: {request.Method} {request.RequestUri}");
            }

            private HttpResponseMessage Write(HttpRequestMessage request, string body, string path)
            {
                WriteCount++;
                if (AddBackingField && request.Method == HttpMethod.Post &&
                    path.EndsWith("/fields/createfieldasxml", StringComparison.OrdinalIgnoreCase))
                {
                    StringAssert.Contains(path.ToLowerInvariant(), libraryId.ToString().ToLowerInvariant());
                    using var json = JsonDocument.Parse(body);
                    var submittedSchema = XElement.Parse(json.RootElement.GetProperty("parameters").GetProperty("SchemaXml").GetString());
                    var source = SiteFields.Single(field => field["InternalName"].ToString() == "CategoryBacking");
                    Assert.AreEqual(SiteId, Guid.Parse((string)submittedSchema.Attribute("ID")));
                    Assert.AreEqual("CategoryBacking", (string)submittedSchema.Attribute("Name"));
                    Assert.AreEqual("Note", (string)submittedSchema.Attribute("Type"));
                    Assert.IsTrue(XNode.DeepEquals(XElement.Parse(source["SchemaXml"].ToString()), submittedSchema),
                        "The backing column must retain its source site-column schema.");
                    Assert.IsFalse(ListFields.Any(field => (Guid)field["Id"] == SiteId),
                        "The backing column must only be provisioned once.");
                    var field = new Dictionary<string, object>(source)
                    {
                        ["odata.id"] = $"{WebUrl}/_api/Web/Lists(guid'{libraryId}')/Fields(guid'{SiteId}')"
                    };
                    ListFields.Add(field);
                    return CoreTestContext.Json(field);
                }
                throw new InvalidOperationException($"Unexpected server write: {request.Method} {request.RequestUri}");
            }
        }

        private static string CategorySchema()
        {
            return $"<Field ID='{WebId}' Name='Wiki_x0020_Page_x0020_Categories' Type='TaxonomyFieldTypeMulti' Mult='TRUE'>" +
                "<Customization><ArrayOfProperty>" +
                $"<Property><Name>SspId</Name><Value>{SiteId}</Value></Property>" +
                $"<Property><Name>TermSetId</Name><Value>{SiteId}</Value></Property>" +
                $"<Property><Name>TextField</Name><Value>{SiteId}</Value></Property>" +
                "</ArrayOfProperty></Customization></Field>";
        }
    }
}
