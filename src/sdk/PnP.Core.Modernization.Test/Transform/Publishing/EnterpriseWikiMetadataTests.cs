using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Extensions;



using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

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
            using (var context = CoreTestContext.Create("https://contoso.sharepoint.com/sites/wiki"))
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
            using (var context = CoreTestContext.Create("https://contoso.sharepoint.com/sites/wiki"))
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
        public void TaxonomyMetadataLoadsThroughTheFieldCollection()
        {
            var id = Guid.NewGuid();
            var termSetId = Guid.NewGuid();
            var storeId = Guid.NewGuid();
            using var context = CoreTestContext.Create();
            var list = CoreTestContext.Item(context).ParentList;
            CoreTestContext.SetResponse(context, (request, _) =>
            {
                Assert.AreEqual(HttpMethod.Get, request.Method);
                StringAssert.EndsWith(request.RequestUri.AbsolutePath, "/Fields");
                StringAssert.Contains(Uri.UnescapeDataString(request.RequestUri.Query), id.ToString());
                return CoreTestContext.Json(new
                {
                    value = new[] { new
                    {
                        Id = id, InternalName = "Categories", TypeAsString = "TaxonomyFieldTypeMulti",
                        TermSetId = termSetId, SspId = storeId, AllowMultipleValues = true
                    } }
                });
            });
            var field = PublishingMetadataTransformator.LoadTaxonomyField(list, id);
            Assert.AreEqual(termSetId, field.TermSetId);
            Assert.AreEqual(storeId, field.SspId);
            Assert.IsTrue(field.AllowMultipleValues);
        }

        [TestMethod]
        public void EmptyCoreUserSentinelIsNotARatingVoter()
        {
            var fields = History(Array.Empty<int>(), "", 0, 0);
            fields["RatedBy"] = UserValue(-1);
            Assert.AreEqual(0, EnterpriseWikiRatings.Read(fields).Count);
            fields["Ratings"] = "5,";
            fields["RatingCount"] = 1;
            fields["AverageRating"] = 5;
            Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiRatings.Read(fields));
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void UnloadedTaxonomyWrapperRequiresExplicitEmptyServerValue(bool empty)
        {
            using var context = CoreTestContext.Create();
            var item = CoreTestContext.Item(context);
            var value = new Mock<IFieldTaxonomyValue>();
            value.SetupGet(term => term.TermId).Throws(new ClientException(ErrorType.PropertyNotLoaded, "TermId"));
            item["Categories"] = value.Object;
            var field = Mock.Of<IField>(definition => definition.InternalName == "Categories" && definition.EntityPropertyName == "Categories");
            CoreTestContext.SetResponse(context, (request, _) =>
            {
                Assert.AreEqual(HttpMethod.Get, request.Method);
                return CoreTestContext.Json(new { Categories = empty ? Array.Empty<string>() : new[] { "A nonempty category" } });
            });
            if (empty)
            {
                Assert.IsNull(field.ReadTaxonomyValue(item));
                Assert.AreSame(value.Object, item["Categories"], "Reading an empty field must not modify the source item.");
            }
            else
            {
                Assert.ThrowsException<InvalidOperationException>(() => field.ReadTaxonomyValue(item));
            }
        }

        [TestMethod]
        public void NonemptyScalarUserDoesNotMasqueradeAsEmptyHistory()
        {
            var fields = History(Array.Empty<int>(), "", 0, 0);
            fields["RatedBy"] = UserValue(41);
            Assert.ThrowsException<InvalidOperationException>(() => EnterpriseWikiRatings.Read(fields));
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
                var source = response.CreateItem(context);
                response.Values = History(Array.Empty<int>(), "", 0, 0);
                foreach (var name in response.Values.Keys.Where(name => omitted == "all" || name == omitted).ToArray())
                {
                    response.Values.Remove(name);
                }
                source.LoadBatch();
                context.Execute();
                Assert.IsFalse(source.Values.ContainsKey(omitted == "all" ? "RatedBy" : omitted));

                response.Values = RatingResponseProvider.VotedHistory();
                var snapshot = EnterpriseWikiRatings.Read(source);

                Assert.AreEqual(2, response.RequestCount);
                var selection = Uri.UnescapeDataString(response.LastRequest.Query).TrimStart('?')
                    .Split('&').SingleOrDefault(part => part.StartsWith("$select=", StringComparison.OrdinalIgnoreCase));
                // Core currently reloads expando fields with a full-item GET. If a projection is used,
                // all four fields must still come from the same request, not just the missing field.
                if (selection != null)
                {
                    var selectedFields = selection.Substring("$select=".Length).Split(',');
                    foreach (var name in new[] { "RatedBy", "Ratings", "RatingCount", "AverageRating" })
                    {
                        Assert.IsTrue(selectedFields.Contains("*") || selectedFields.Contains(name) || selectedFields.Contains(name + "Id"),
                            $"Missing REST selection of {name}.");
                    }
                }
                Assert.IsTrue(response.LastRequest.AbsolutePath.EndsWith("/items(1)", StringComparison.OrdinalIgnoreCase));
                Assert.IsInstanceOfType<IFieldValueCollection>(source["RatedBy"]);
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
                var source = response.CreateItem(context);
                source.LoadBatch();
                context.Execute();
                var snapshot = EnterpriseWikiRatings.Read(source);
                Assert.AreEqual(1, response.RequestCount);
                Assert.AreEqual(2, snapshot.Count);
                Assert.AreEqual(3.5, snapshot.Average);
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void MissingRestRatingFieldsUseCoreProtocolAndStillFailClosed(bool complete)
        {
            using (var context = CoreTestContext.Create())
            {
                var source = new Mock<IListItem>();
                source.SetupGet(item => item.Values).Returns(CoreTestContext.Item(context).Values);
                source.SetupGet(item => item.Id).Returns(1);
                source.SetupGet(item => item.PnPContext).Returns(context);
                var loaded = CoreTestContext.Item(context, 2);
                if (complete)
                    foreach (var pair in History(Array.Empty<int>(), "", 0, 0)) loaded[pair.Key] = pair.Value;
                var result = new Mock<ICamlQueryCsomResult>();
                result.SetupGet(value => value.Items).Returns(new[] { loaded });
                var list = new Mock<IList>();
                list.Setup(value => value.LoadItemsByCamlQueryViaCsom(It.IsAny<CamlQueryOptions>())).Returns(result.Object);
                source.SetupGet(item => item.ParentList).Returns(list.Object);
                if (complete)
                    Assert.AreEqual(0, EnterpriseWikiRatings.Read(source.Object).Count);
                else
                    StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() =>
                        EnterpriseWikiRatings.Read(source.Object)).Message, "missing or not loaded");
                list.Verify(value => value.LoadItemsByCamlQueryViaCsom(It.Is<CamlQueryOptions>(query =>
                    query.ViewXml.Contains("RatedBy") && query.ViewXml.Contains("Ratings") &&
                    query.ViewXml.Contains("RatingCount") && query.ViewXml.Contains("AverageRating") &&
                    query.ViewXml.Contains("<RowLimit>1</RowLimit>"))), Times.Once);
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
            var target = source.MapVoters(v => UserValue(v.LookupId == 41 ? 8 : 99));
            CollectionAssert.AreEqual(new[] { 8, 99 }, target.Voters.Select(v => v.LookupId).ToArray());
            CollectionAssert.AreEqual(new[] { 2, 5 }, target.Scores);
            Assert.AreEqual("2,5,", target.Ratings);
            Assert.AreEqual(3.5, target.Average);
            CollectionAssert.AreEqual(new[] { 41, 12 }, source.Voters.Select(v => v.LookupId).ToArray());
            Assert.ThrowsException<InvalidOperationException>(() => source.MapVoters(_ => null));
            Assert.ThrowsException<InvalidOperationException>(() => source.MapVoters(_ => UserValue(8)));
        }

        private sealed class RatingResponseProvider
        {
            private const string Url = "https://contoso.sharepoint.com/sites/wiki";
            private Guid listId;
            internal Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();
            internal int RequestCount { get; private set; }
            internal Uri LastRequest { get; private set; }

            internal PnPContext CreateContext() => CoreTestContext.Create(Url, Respond);

            internal IListItem CreateItem(PnPContext context)
            {
                var item = CoreTestContext.Item(context);
                listId = ((IList)item.Parent.Parent).Id;
                return item;
            }

            internal static Dictionary<string, object> VotedHistory() => History(new[] { 41, 12 }, "2,5,", 2, 3.5);

            private static object[] Fields() => new[]
            {
                new { Id = new Guid("4d64b067-08c3-43dc-a87b-8b8e01673313"), InternalName = "RatedBy", Title = "Rated By", FieldTypeKind = 20, TypeAsString = "UserMulti" },
                new { Id = new Guid("434f51fb-ffd2-4a0e-a03b-ca3131ac67ba"), InternalName = "Ratings", Title = "Ratings", FieldTypeKind = 3, TypeAsString = "Note" },
                new { Id = new Guid("b1996002-9167-45e5-a4df-b2c41c6723c7"), InternalName = "RatingCount", Title = "Rating Count", FieldTypeKind = 9, TypeAsString = "RatingCount" },
                new { Id = new Guid("5a14d1ab-1513-48c7-97b3-657a5ba6c742"), InternalName = "AverageRating", Title = "Average Rating", FieldTypeKind = 9, TypeAsString = "AverageRating" }
            };

            private HttpResponseMessage Respond(HttpRequestMessage request, string body)
            {
                Assert.AreEqual(HttpMethod.Get, request.Method, "Reading rating history must not write to SharePoint.");
                var path = request.RequestUri.AbsolutePath;
                if (path.EndsWith("/fields", StringComparison.OrdinalIgnoreCase))
                {
                    return CoreTestContext.Json(new { value = Fields() });
                }
                if (!path.Contains("/items", StringComparison.OrdinalIgnoreCase))
                {
                    var list = new
                    {
                        Id = listId,
                        Title = "Pages",
                        ListItemEntityTypeFullName = "SP.Data.PagesItem",
                        Fields = Fields()
                    };
                    if (path.EndsWith("/lists", StringComparison.OrdinalIgnoreCase))
                    {
                        return CoreTestContext.Json(new { value = new[] { list } });
                    }
                    if (path.Contains("/lists(", StringComparison.OrdinalIgnoreCase))
                    {
                        return CoreTestContext.Json(list);
                    }
                    throw new InvalidOperationException($"Unexpected rating fixture request: {request.Method} {request.RequestUri}");
                }

                RequestCount++;
                LastRequest = request.RequestUri;
                var values = new Dictionary<string, object>(Values)
                {
                    ["odata.type"] = "SP.Data.PagesItem",
                    ["odata.id"] = $"{Url}/_api/Web/Lists(guid'{listId}')/Items(1)",
                    ["Id"] = 1
                };
                if (values.TryGetValue("RatedBy", out var voters))
                {
                    values.Remove("RatedBy");
                    values["RatedById"] = voters == null ? null :
                        ((IEnumerable<IFieldUserValue>)voters).Select(voter => voter.LookupId).ToArray();
                }
                return path.EndsWith("/items", StringComparison.OrdinalIgnoreCase)
                    ? CoreTestContext.Json(new { value = new[] { values } })
                    : CoreTestContext.Json(values);
            }
        }

        private static IFieldUserValue UserValue(int id) => Mock.Of<IFieldUserValue>(user => user.LookupId == id);

        private static Dictionary<string, object> History(int[] voters, string scores, int count, double average)
        {
            return new Dictionary<string, object>
            {
                ["RatedBy"] = voters.Select(UserValue).ToArray(),
                ["Ratings"] = scores,
                ["RatingCount"] = count,
                ["AverageRating"] = average
            };
        }
    }
}
