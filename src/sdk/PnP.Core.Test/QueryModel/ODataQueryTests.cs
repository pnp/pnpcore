using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class ODataQueryTests
    {
        [TestMethod]
        public void TestFlatFilters()
        {
            var expected = "$filter=displayName eq 'Test 01' and description ne 'Test 02'&$top=10&$skip=5";

            ODataQuery<IWeb> query = new ODataQuery<IWeb>();
            query.Top = 10;
            query.Skip = 5;
            
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

            var actual = query.ToString();
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInDepthFilters()
        {
            var expected = "$filter=displayName eq 'Test 01' or (sharepointIds eq 7 or AuthorID eq 15 and ModifiedBy eq 'paolo@piasysdev.onmicrosoft.com') and description ne 'Test 02'&$top=10&$skip=5";

            ODataQuery<IWeb> query = new ODataQuery<IWeb>();
            query.Top = 10;
            query.Skip = 5;

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

            var actual = query.ToString();
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestInDepthFiltersSingleGroup()
        {
            var expected = "$filter=(sharepointIds eq 1 or AuthorID eq 2)";

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

            var actual = query.ToString();
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }
    }
}