using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Publishing;
using PnP.Framework.Utilities.UnitTests.Model;
using PnP.Framework.Utilities.UnitTests.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PnP.Core.Modernization.Tests.Transform.Publishing
{
    [TestClass]
    [TestCategory("Offline")]
    public class EnterpriseWikiMetadataTests
    {
        [TestMethod]
        public void OptedInEnterpriseWikiMappingCopiesAllFiveSameNameFields()
        {
            var model = new PageLayoutManager().LoadDefaultPageLayoutMappingFile(true);
            var layout = model.PageLayouts.Single(l => l.Name == "EnterpriseWiki");
            foreach (var name in new[] { "Wiki_x0020_Page_x0020_Categories", "RatedBy", "Ratings", "RatingCount", "AverageRating" })
            {
                Assert.AreEqual(name, layout.MetaData.Field.Single(f => f.Name == name).TargetFieldName);
            }
            Assert.IsTrue(EnterpriseWikiRatings.UsesSameNameMapping(layout));
            Assert.IsFalse(EnterpriseWikiRatings.UsesSameNameMapping(model.PageLayouts.First(l => l.Name != "EnterpriseWiki")));
        }

        [TestMethod]
        public void CustomRatingFunctionsAreNotOverridden()
        {
            var layout = new PageLayoutManager().LoadDefaultPageLayoutMappingFile(true).PageLayouts.Single(l => l.Name == "EnterpriseWiki");
            layout.MetaData.Field.Single(f => f.Name == "Ratings").Functions = "CustomRatings()";
            Assert.IsFalse(EnterpriseWikiRatings.UsesSameNameMapping(layout));
        }

        [TestMethod]
        public void SwitchingFlagRestoresLegacyMappingAndSidebarBehavior()
        {
            using (var context = new ClientContext("https://contoso.sharepoint.com/sites/wiki"))
            {
                var transformator = new PublishingPageTransformator(context, context, new PageTransformation(), null);
                var information = new PublishingPageTransformationInformation(null) { IsEnterpriseWikiPage = true };
                var legacy = transformator.GetPublishingModel(information);
                var legacyLayout = legacy.PageLayouts.Single(l => l.Name == "EnterpriseWiki");
                Assert.IsFalse(EnterpriseWikiRatings.UsesSameNameMapping(legacyLayout));
                Assert.IsTrue(legacyLayout.MetaData.Field.All(f => string.IsNullOrEmpty(f.TargetFieldName)));
                Assert.IsFalse(information.ShouldIncludeEnterpriseWikiSidebar);

                information.EnableEnterpriseWikiCapabilities = true;
                var enabled = transformator.GetPublishingModel(information);
                Assert.IsTrue(EnterpriseWikiRatings.UsesSameNameMapping(enabled.PageLayouts.Single(l => l.Name == "EnterpriseWiki")));
                Assert.IsTrue(information.ShouldIncludeEnterpriseWikiSidebar);

                information.EnableEnterpriseWikiCapabilities = false;
                Assert.AreSame(legacy, transformator.GetPublishingModel(information));
                Assert.IsTrue(legacyLayout.MetaData.Field.All(f => string.IsNullOrEmpty(f.TargetFieldName)));
                Assert.IsFalse(information.ShouldIncludeEnterpriseWikiSidebar);
            }
        }

        [TestMethod]
        public void CustomMetadataMappingIsHonoredInBothModes()
        {
            var custom = new PageLayoutManager().LoadDefaultPageLayoutMappingFile();
            var layout = custom.PageLayouts.Single(l => l.Name == "EnterpriseWiki");
            layout.MetaData.Field.Single(f => f.Name == EnterpriseWikiPagePreparation.CategoryFieldName).TargetFieldName = "CustomCategories";
            custom.PageLayouts = new[] { layout };
            using (var context = new ClientContext("https://contoso.sharepoint.com/sites/wiki"))
            {
                var transformator = new PublishingPageTransformator(context, context, new PageTransformation(), custom);
                foreach (var enabled in new[] { false, true })
                {
                    var information = new PublishingPageTransformationInformation(null) { EnableEnterpriseWikiCapabilities = enabled };
                    var selected = transformator.GetPublishingModel(information).PageLayouts.Single(l => l.Name == "EnterpriseWiki");
                    Assert.AreEqual("CustomCategories", selected.MetaData.Field.Single(f => f.Name == EnterpriseWikiPagePreparation.CategoryFieldName).TargetFieldName);
                    Assert.IsFalse(EnterpriseWikiRatings.UsesSameNameMapping(selected));
                }
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void ExplicitlyLoadedEmptyHistoryIsValid(bool nullValues)
        {
            var fields = History(Array.Empty<int>(), "", 0, 0);
            if (nullValues)
            {
                foreach (var name in fields.Keys.ToArray())
                {
                    fields[name] = null;
                }
            }
            var snapshot = EnterpriseWikiRatings.Read(fields);
            Assert.AreEqual(0, snapshot.Voters.Length);
            Assert.AreEqual(0, snapshot.Scores.Length);
            Assert.AreEqual(0, snapshot.Count);
            Assert.AreEqual(0, snapshot.Average);
        }

        [TestMethod]
        public void UnloadedFieldsAreNotAnEmptyHistory()
        {
            Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiRatings.Read(new Dictionary<string, object>()));
            foreach (var name in new[] { "RatedBy", "Ratings", "RatingCount", "AverageRating" })
            {
                var fields = History(Array.Empty<int>(), "", 0, 0);
                fields.Remove(name);
                var exception = Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiRatings.Read(fields));
                StringAssert.Contains(exception.Message, name);
            }
        }

        [DataTestMethod]
        [DataRow("all")]
        [DataRow("RatedBy")]
        [DataRow("Ratings")]
        [DataRow("RatingCount")]
        [DataRow("AverageRating")]
        public void PartialSourceReloadsAllRatingFieldsTogether(string omitted)
        {
            var response = new RatingResponseProvider();
            using (var context = response.CreateContext())
            {
                var source = context.Web.Lists.GetByTitle("Pages").GetItemById(1);
                response.Values = History(Array.Empty<int>(), "", 0, 0);
                foreach (var name in response.Values.Keys.Where(name => omitted == "all" || name == omitted).ToArray())
                {
                    response.Values.Remove(name);
                }
                context.Load(source);
                context.ExecuteQueryRetry();
                Assert.IsFalse(source.FieldValues.ContainsKey(omitted == "all" ? "RatedBy" : omitted));

                response.Values = RatingResponseProvider.VotedHistory();
                var snapshot = EnterpriseWikiRatings.Read(source);

                Assert.AreEqual(2, response.RequestCount);
                var request = XElement.Parse(response.LastRequest);
                foreach (var name in new[] { "RatedBy", "Ratings", "RatingCount", "AverageRating" })
                {
                    Assert.IsTrue(request.Descendants().Any(e => e.Name.LocalName == "Property" &&
                        (string)e.Attribute("Name") == name), $"Missing explicit load of {name}.");
                }
                CollectionAssert.AreEqual(new[] { 41, 12 }, snapshot.Voters.Select(v => v.LookupId).ToArray());
                CollectionAssert.AreEqual(new[] { 2, 5 }, snapshot.Scores);
                Assert.AreEqual(2, snapshot.Count);
                Assert.AreEqual(3.5, snapshot.Average);
            }
        }

        [TestMethod]
        public void FullyLoadedSourceNeedsNoAdditionalRequest()
        {
            var response = new RatingResponseProvider { Values = RatingResponseProvider.VotedHistory() };
            using (var context = response.CreateContext())
            {
                var source = context.Web.Lists.GetByTitle("Pages").GetItemById(1);
                context.Load(source);
                context.ExecuteQueryRetry();
                var snapshot = EnterpriseWikiRatings.Read(source);
                Assert.AreEqual(1, response.RequestCount);
                Assert.AreEqual(2, snapshot.Count);
                Assert.AreEqual(3.5, snapshot.Average);
            }
        }

        [TestMethod]
        public void ReloadThatStillOmitsRatingFieldsFailsClosed()
        {
            var response = new RatingResponseProvider();
            using (var context = response.CreateContext())
            {
                var source = context.Web.Lists.GetByTitle("Pages").GetItemById(1);
                context.Load(source);
                context.ExecuteQueryRetry();
                var exception = Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiRatings.Read(source));
                StringAssert.Contains(exception.Message, "missing or not loaded");
                Assert.AreEqual(2, response.RequestCount);
            }
        }

        [DataTestMethod]
        [DataRow("6,")]
        [DataRow("3,,")]
        public void RejectsInvalidScoreSerialization(string ratings)
        {
            Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiRatings.Read(History(new[] { 1 }, ratings, 1, 3)));
        }

        [TestMethod]
        public void RejectsIncompleteOrInconsistentHistory()
        {
            Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiRatings.Read(History(Array.Empty<int>(), "5,5,", 2, 5)));
            Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiRatings.Read(History(new[] { 1 }, "3,", 2, 3)));
            Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiRatings.Read(History(new[] { 1 }, "3,", 1, 4)));
            Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiRatings.Read(History(new[] { 1, 1 }, "3,3,", 2, 3)));
        }

        [TestMethod]
        public void RemapsUsersWithoutReorderingOrDroppingScores()
        {
            var source = EnterpriseWikiRatings.Read(History(new[] { 41, 12 }, "2,5,", 2, 3.5));
            var target = source.MapVoters(v => new FieldUserValue { LookupId = v.LookupId == 41 ? 8 : 99 });
            CollectionAssert.AreEqual(new[] { 8, 99 }, target.Voters.Select(v => v.LookupId).ToArray());
            CollectionAssert.AreEqual(new[] { 2, 5 }, target.Scores);
            Assert.AreEqual("2,5,", target.Ratings);
            Assert.AreEqual(3.5, target.Average);
            CollectionAssert.AreEqual(new[] { 41, 12 }, source.Voters.Select(v => v.LookupId).ToArray());
            Assert.ThrowsException<InvalidOperationException>(() => source.MapVoters(_ => null));
            Assert.ThrowsException<InvalidOperationException>(() => source.MapVoters(_ => new FieldUserValue { LookupId = 8 }));
        }

        private sealed class RatingResponseProvider : IMockResponseProvider
        {
            private const string Url = "https://contoso.sharepoint.com/sites/wiki";
            internal Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();
            internal int RequestCount { get; private set; }
            internal string LastRequest { get; private set; }

            internal ClientContext CreateContext() =>
                new ClientContext(Url) { WebRequestExecutorFactory = new MockWebRequestExecutorFactory(this) };

            internal static Dictionary<string, object> VotedHistory() => new Dictionary<string, object>
            {
                ["RatedBy"] = new[]
                {
                    new { _ObjectType_ = "SP.FieldUserValue", LookupId = 41, LookupValue = "First voter" },
                    new { _ObjectType_ = "SP.FieldUserValue", LookupId = 12, LookupValue = "Second voter" }
                },
                ["Ratings"] = "2,5,",
                ["RatingCount"] = 2,
                ["AverageRating"] = 3.5
            };

            public string GetResponse(string url, string verb, string body)
            {
                RequestCount++;
                LastRequest = body;
                var values = new Dictionary<string, object>(Values)
                {
                    ["_ObjectType_"] = "SP.ListItem",
                    ["_ObjectIdentity_"] = "RatingSource",
                    ["ID"] = 1
                };
                if (values.TryGetValue("RatedBy", out var voters))
                {
                    values.Remove("RatedBy");
                    values["RatedBy$SP.FieldUserValue$  Array"] = voters;
                }
                var provider = new MockEntryResponseProvider();
                provider.ResponseEntries.Add(new MockResponseEntry<object> { Url = Url, Method = "GetItemById", ReturnValue = values });
                provider.ResponseEntries.Add(new MockResponseEntry<object> { Url = Url, PropertyName = "RatingSource", ReturnValue = values });
                return provider.GetResponse(url, verb, body);
            }
        }

        private static Dictionary<string, object> History(int[] voters, string scores, int count, double average)
        {
            return new Dictionary<string, object>
            {
                ["RatedBy"] = voters.Select(id => new FieldUserValue { LookupId = id }).ToArray(),
                ["Ratings"] = scores,
                ["RatingCount"] = count,
                ["AverageRating"] = average
            };
        }
    }
}
