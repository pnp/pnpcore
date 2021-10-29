using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class QueryableExtensionsTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task TestAsBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var siteAssets = await context.Web.Lists.QueryProperties(
                                    p => p.Title, p => p.TemplateType,
                                    p => p.ContentTypes.QueryProperties(
                                        p => p.Name,
                                        p => p.FieldLinks.QueryProperties(p => p.Name)))
                                .Where(p => p.Title == "Site Assets")
                                .AsBatchAsync();

                var siteAssets2 = context.Web.Lists.QueryProperties(
                                    p => p.Title, p => p.TemplateType,
                                    p => p.ContentTypes.QueryProperties(
                                        p => p.Name,
                                        p => p.FieldLinks.QueryProperties(p => p.Name)))
                                .Where(p => p.Title == "Site Assets")
                                .AsBatch();

                var batch = context.NewBatch();
                siteAssets = await context.Web.Lists.QueryProperties(
                                    p => p.Title, p => p.TemplateType,
                                    p => p.ContentTypes.QueryProperties(
                                        p => p.Name,
                                        p => p.FieldLinks.QueryProperties(p => p.Name)))
                                .Where(p => p.Title == "Site Assets")
                                .AsBatchAsync(batch);

                siteAssets2 = context.Web.Lists.QueryProperties(
                                    p => p.Title, p => p.TemplateType,
                                    p => p.ContentTypes.QueryProperties(
                                        p => p.Name,
                                        p => p.FieldLinks.QueryProperties(p => p.Name)))
                                .Where(p => p.Title == "Site Assets")
                                .AsBatch(batch);
                await context.ExecuteAsync(batch);

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    IQueryable<Model.SharePoint.IList> bla = null;
                    bla.AsBatch();
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IQueryable<Model.SharePoint.IList> bla = null;
                    await bla.AsBatchAsync();
                });

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    var batch = context.NewBatch();
                    IQueryable<Model.SharePoint.IList> bla = null;
                    bla.AsBatch(batch);
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    var batch = context.NewBatch();
                    IQueryable<Model.SharePoint.IList> bla = null;
                    await bla.AsBatchAsync(batch);
                });
            }
        }

        [TestMethod]
        public async Task TestQueryProperties()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.ThrowsException<InvalidOperationException>(() =>
                {
                    ISupportQuery<Model.SharePoint.IList> bla = null;
                    bla.QueryProperties();
                });

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    IQueryable<Model.SharePoint.IList> bla = null;
                    bla.QueryProperties();
                });

                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    IQueryable<Model.SharePoint.IList> bla = context.Web.Lists;
                    bla.QueryProperties(null);
                });
            }
        }


        [TestMethod]
        public async Task TestFirst()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var siteAssets = await context.Web.Lists.QueryProperties(
                                    p => p.Title, p => p.TemplateType,
                                    p => p.ContentTypes.QueryProperties(
                                        p => p.Name,
                                        p => p.FieldLinks.QueryProperties(p => p.Name)))
                                .Where(p => p.Title == "Site Assets")
                                .FirstAsync();

                var siteAssets2 = await context.Web.Lists.QueryProperties(
                                    p => p.Title, p => p.TemplateType,
                                    p => p.ContentTypes.QueryProperties(
                                        p => p.Name,
                                        p => p.FieldLinks.QueryProperties(p => p.Name)))
                                .Where(p => p.Title == "Site Assets")
                                .FirstOrDefaultAsync();


                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IQueryable<Model.SharePoint.IList> bla = null;
                    await bla.FirstAsync();
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IQueryable<Model.SharePoint.IList> bla = null;
                    await bla.FirstAsync(null);
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IQueryable<Model.SharePoint.IList> bla = context.Web.Lists;
                    await bla.FirstAsync(null);
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IQueryable<Model.SharePoint.IList> bla = null;
                    await bla.FirstOrDefaultAsync(null);
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IQueryable<Model.SharePoint.IList> bla = context.Web.Lists;
                    await bla.FirstOrDefaultAsync(null);
                });

            }
        }

        [TestMethod]
        public async Task TestToDictionary()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var d1 = await context.Web.Lists.QueryProperties(
                                    p => p.Title, p => p.TemplateType,
                                    p => p.ContentTypes.QueryProperties(
                                        p => p.Name,
                                        p => p.FieldLinks.QueryProperties(p => p.Name)))                                
                                .ToDictionaryAsync(o => o.Id);

                var d2 = await context.Web.Lists.QueryProperties(
                                    p => p.Title, p => p.TemplateType,
                                    p => p.ContentTypes.QueryProperties(
                                        p => p.Name,
                                        p => p.FieldLinks.QueryProperties(p => p.Name)))                                
                                .ToDictionaryAsync(o => o.Id, p => p.Title);


                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IQueryable<Model.SharePoint.IList> bla = null;
                    await bla.ToDictionaryAsync(o => o.Id);
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IQueryable<Model.SharePoint.IList> bla = context.Web.Lists;
                    Func<Model.SharePoint.IList, int> keySelector = null;
                    await bla.ToDictionaryAsync(keySelector);
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IQueryable<Model.SharePoint.IList> bla = context.Web.Lists;
                    Func<Model.SharePoint.IList, string> selector = null;
                    await bla.ToDictionaryAsync(o => o.Id, selector);
                });
            }
        }

        [TestMethod]
        public async Task TestForEachAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.Lists.ForEachAsync(async x => 
                {
                    Assert.IsTrue(!string.IsNullOrEmpty(x.Title));
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IQueryable<Model.SharePoint.IList> bla = null;
                    await bla.ForEachAsync(async x => { });
                });

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    await context.Web.Lists.ForEachAsync(null);
                });
            }
        }

        [TestMethod]
        public async Task TestAsAsyncEnmerable()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var b = context.Web.Lists.AsAsyncEnumerable();

                await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                {
                    IQueryable<Model.SharePoint.IList> bla = null;
                    var a = bla.AsAsyncEnumerable();
                });
            }
        }

        [TestMethod]
        public async Task TestAsRequested()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
            {
                IDataModelCollection<Model.SharePoint.IList> bla = null;
                var a = bla.AsRequested();
            });
        }
    }
}