using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using System.Collections.Generic;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class ODataQueryTests
    {
        [TestMethod]
        public void TestFlatFiltersGraph()
        {
            var expected = "$filter=displayName eq 'Test 01' and description ne 'Test 02'";
            TestFlatFilterByPlatform(expected, ODataTargetPlatform.Graph);
        }

        [TestMethod]
        public void TestFlatFiltersSPORest()
        {
            var expected = "$filter=Title eq 'Test 01' and Description ne 'Test 02'&$top=10&$skip=5";
            TestFlatFilterByPlatform(expected, ODataTargetPlatform.SPORest);
        }

        private static void TestFlatFilterByPlatform(string expected, ODataTargetPlatform platform)
        {
            ODataQuery<IWeb> query = new ODataQuery<IWeb>
            {
                Top = 10,
                Skip = 5
            };

            // Add one filter
            query.Filters.Add(new FilterItem
            {
                Field = "Title",
                Criteria = FilteringCriteria.Equal,
                Value = "Test 01",
            });

            // Add another filter
            query.Filters.Add(new FilterItem
            {
                Field = "Description",
                Criteria = FilteringCriteria.NotEqual,
                Value = "Test 02",
                ConcatOperator = FilteringConcatOperator.AND
            });

            var actual = query.ToQueryString(platform);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInDepthFiltersGraph()
        {
            var expected = "$filter=displayName eq 'Test 01' or (sharepointIds eq 7 or AuthorID eq 15 and ModifiedBy eq 'paolo@piasysdev.onmicrosoft.com') and description ne 'Test 02'";
            TestInDepthFiltersByPlatform(expected, ODataTargetPlatform.Graph);
        }

        [TestMethod]
        public void TestInDepthFiltersSPORest()
        {
            var expected = "$filter=Title eq 'Test 01' or (Id eq 7 or AuthorID eq 15 and ModifiedBy eq 'paolo@piasysdev.onmicrosoft.com') and Description ne 'Test 02'&$top=10&$skip=5";
            TestInDepthFiltersByPlatform(expected, ODataTargetPlatform.SPORest);
        }

        private static void TestInDepthFiltersByPlatform(string expected, ODataTargetPlatform platform)
        {
            ODataQuery<IWeb> query = new ODataQuery<IWeb>
            {
                Top = 10,
                Skip = 5
            };

            // Add one filter
            query.Filters.Add(new FilterItem
            {
                Field = "Title",
                Criteria = FilteringCriteria.Equal,
                Value = "Test 01",
            });

            // Add a group filter
            query.Filters.Add(new FiltersGroup(
                new List<ODataFilter>(new ODataFilter[]
                {
                    new FilterItem
                        {
                            Field = "ID",
                            Criteria = FilteringCriteria.Equal,
                            Value = 7,
                        },
                    new FilterItem
                        {
                            Field = "AuthorID",
                            Criteria = FilteringCriteria.Equal,
                            Value = 15,
                            ConcatOperator = FilteringConcatOperator.OR
                        },
                    new FilterItem
                        {
                            Field = "ModifiedBy",
                            Criteria = FilteringCriteria.Equal,
                            Value = "paolo@piasysdev.onmicrosoft.com",
                            ConcatOperator = FilteringConcatOperator.AND
                        },
                })
                )
            {
                ConcatOperator = FilteringConcatOperator.OR,
            });

            // Add another filter
            query.Filters.Add(new FilterItem
            {
                Field = "Description",
                Criteria = FilteringCriteria.NotEqual,
                Value = "Test 02",
                ConcatOperator = FilteringConcatOperator.AND
            });

            var actual = query.ToQueryString(platform);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInDepthFiltersSingleGroupGraph()
        {
            var expected = "$filter=(sharepointIds eq 1 or AuthorID eq 2)";

            TestInDepthFiltersSingleGroupByPlatform(expected, ODataTargetPlatform.Graph);
        }

        [TestMethod]
        public void TestInDepthFiltersSingleGroupSPORest()
        {
            var expected = "$filter=(Id eq 1 or AuthorID eq 2)";

            TestInDepthFiltersSingleGroupByPlatform(expected, ODataTargetPlatform.SPORest);
        }

        private static void TestInDepthFiltersSingleGroupByPlatform(string expected, ODataTargetPlatform platform)
        {
            ODataQuery<IWeb> query = new ODataQuery<IWeb>();

            // Add a group filter
            query.Filters.Add(new FiltersGroup(
                new List<ODataFilter>(new ODataFilter[]
                {
                    new FilterItem
                        {
                            Field = "ID",
                            Criteria = FilteringCriteria.Equal,
                            Value = 1,
                        },
                    new FilterItem
                        {
                            Field = "AuthorID",
                            Criteria = FilteringCriteria.Equal,
                            Value = 2,
                            ConcatOperator = FilteringConcatOperator.OR
                        }
                })
                )
            {
                ConcatOperator = FilteringConcatOperator.OR,
            });

            var actual = query.ToQueryString(platform);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }
    }
}