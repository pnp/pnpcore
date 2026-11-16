using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Publishing;
using PnP.Framework.Utilities.UnitTests.Model;
using PnP.Framework.Utilities.UnitTests.Web;

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
            field["SchemaXmlWithResourceTokens"] = field["SchemaXmlWithResourceTokens"].ToString().Replace("Type='AverageRating'", "Type='Number'");
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
                    _ObjectType_ = "SP.Feature",
                    DefinitionId = "/Guid(94c94ca6-b32f-4da9-a9e3-1f3d343d7ecb)/"
                });
            }
            using (var context = fixture.CreateContext())
            {
                var exception = Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiPagePreparation.Prepare(context));
                StringAssert.Contains(exception.Message, "Site Pages feature is not enabled");
                StringAssert.Contains(exception.Message, context.Url);
                StringAssert.Contains(exception.Message, EnterpriseWikiPagePreparation.SitePagesFeatureId.ToString());
                StringAssert.Contains(exception.Message, "Manage site features");
            }
            Assert.AreEqual(1, fixture.RequestCount);
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
            Assert.AreEqual(1, fixture.RequestCount);
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
            Assert.AreEqual(1, fixture.RequestCount);
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
                var exception = Assert.ThrowsException<ClientRequestException>(() => EnterpriseWikiPagePreparation.Prepare(context));
                Assert.IsFalse(exception.Message.Contains("Site Pages feature is not enabled"));
            }
            Assert.AreEqual(1, fixture.RequestCount);
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

        private sealed class PreparationFixture : IMockResponseProvider
        {
            private const string WebUrl = "https://contoso.sharepoint.com/sites/wiki";
            private readonly MockEntryResponseProvider provider = new MockEntryResponseProvider();
            internal readonly List<Dictionary<string, object>> SiteFields = new List<Dictionary<string, object>>();
            internal readonly List<Dictionary<string, object>> ListFields = new List<Dictionary<string, object>>();
            internal readonly List<object> Features = new List<object>();
            internal bool FailRequests { get; set; }
            internal bool AddBackingField { get; set; }
            internal int RequestCount { get; private set; }
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
                    if (!hidden)
                    {
                        element.SetAttributeValue("ShowInEditForm", id == WebId ? "TRUE" : "FALSE");
                        element.SetAttributeValue("ShowInNewForm", id == WebId ? "TRUE" : "FALSE");
                    }
                    var field = new Dictionary<string, object>
                    {
                        ["_ObjectType_"] = "SP.Field",
                        ["_ObjectIdentity_"] = $"Field:{id}",
                        ["Id"] = $"/Guid({id})/",
                        ["InternalName"] = name,
                        ["TypeAsString"] = type,
                        ["SchemaXmlWithResourceTokens"] = element.ToString().Replace('"', '\''),
                        ["Hidden"] = hidden,
                        ["ReadOnlyField"] = false
                    };
                    SiteFields.Add(field);
                    ListFields.Add(new Dictionary<string, object>(field));
                }

                var library = new
                {
                    _ObjectType_ = "SP.List",
                    _ObjectIdentity_ = "TargetLibrary",
                    Id = $"/Guid({Guid.NewGuid()})/",
                    BaseTemplate = 119,
                    EffectiveBasePermissions = new { _ObjectType_ = "SP.BasePermissions", High = 2147483647U, Low = 4294967295U },
                    RootFolder = new { _ObjectType_ = "SP.Folder", ServerRelativeUrl = "/sites/wiki/SitePages" }
                };
                AddProperty("Web", new { ServerRelativeUrl = "/sites/wiki" });
                AddProperty("AvailableFields", new { _Child_Items_ = SiteFields });
                AddProperty("Fields", new { _Child_Items_ = ListFields });
                Features.Add(FeatureResponse());
                AddProperty("Features", new { _Child_Items_ = Features });
                AddProperty("Lists", new { _Child_Items_ = new[] { library } });
                AddProperty("TargetLibrary", library);
                AddProperty("ContentTypes", new
                {
                    _Child_Items_ = new[]
                    {
                        new
                        {
                            _ObjectType_ = "SP.ContentType",
                            _ObjectIdentity_ = "SitePageContentType",
                            Id = new { _ObjectType_ = "SP.ContentTypeId", StringValue = BuiltInContentTypeId.ModernArticlePage + "00" + Guid.NewGuid().ToString("N") },
                            Parent = new
                            {
                                _ObjectType_ = "SP.ContentType",
                                Id = new { _ObjectType_ = "SP.ContentTypeId", StringValue = BuiltInContentTypeId.ModernArticlePage }
                            }
                        }
                    }
                });
                AddProperty("FieldLinks", new
                {
                    _Child_Items_ = fields.Select(f => new
                    {
                        _ObjectType_ = "SP.FieldLink",
                        Id = $"/Guid({f.Item1})/",
                        // A native backing field is hidden on the list, not necessarily on its link.
                        Hidden = f.Item1 != SiteId && f.Item4,
                        ReadOnly = false
                    }).ToArray()
                });
                provider.ResponseEntries.Add(new MockResponseEntry<object> { Url = WebUrl, Method = "GetList", ReturnValue = library });
                provider.ResponseEntries.Add(new MockResponseEntry<object> { Url = WebUrl, Method = "Add", ReturnValue = FeatureResponse() });
            }

            internal static object FeatureResponse()
            {
                return new { _ObjectType_ = "SP.Feature", DefinitionId = $"/Guid({EnterpriseWikiPagePreparation.SitePagesFeatureId})/" };
            }

            private void AddProperty(string name, object value)
            {
                provider.ResponseEntries.Add(new MockResponseEntry<object> { Url = WebUrl, PropertyName = name, ReturnValue = value });
            }

            internal ClientContext CreateContext()
            {
                return new ClientContext(WebUrl) { WebRequestExecutorFactory = new MockWebRequestExecutorFactory(this) };
            }

            public string GetResponse(string url, string verb, string body)
            {
                RequestCount++;
                if (FailRequests)
                {
                    throw new InvalidOperationException("Feature query failed.");
                }
                var request = XElement.Parse(body);
                var writes = request.Descendants().Where(e =>
                    (e.Name.LocalName == "Method" && e.Parent.Name.LocalName == "Actions") ||
                    e.Name.LocalName == "SetProperty" ||
                    (e.Name.LocalName == "Method" && (string)e.Attribute("Name") == "Add")).ToList();
                WriteCount += writes.Count;
                if (writes.Count > 0)
                {
                    var write = writes[0];
                    var parent = request.Descendants().FirstOrDefault(e =>
                        (string)e.Attribute("Id") == (string)write.Attribute("ParentId"));
                    if (AddBackingField && writes.Count == 1 && (string)write.Attribute("Name") == "Add" &&
                        (string)parent?.Attribute("Name") == "Fields")
                    {
                        var parameter = write.Descendants().Single(e => e.Name.LocalName == "Parameter");
                        var source = request.Descendants().Single(e =>
                            (string)e.Attribute("Id") == (string)parameter.Attribute("ObjectPathId"));
                        Assert.AreEqual($"Field:{SiteId}", (string)source.Attribute("Name"));
                        var field = new Dictionary<string, object>(SiteFields.Single(f => f["InternalName"].ToString() == "CategoryBacking"));
                        ListFields.Add(field);
                        provider.ResponseEntries.Single(e => e.Method == "Add").ReturnValue = field;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unexpected server write: " + writes[0]);
                    }
                }
                var response = provider.GetResponse(url, verb, body);
                // SharePoint does not include FieldLink.ReadOnly in the default property set.
                if (!request.Descendants().Any(e => e.Name.LocalName == "Property" && (string)e.Attribute("Name") == "ReadOnly"))
                {
                    response = response.Replace(",\"ReadOnly\":false", string.Empty);
                }
                return response;
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
