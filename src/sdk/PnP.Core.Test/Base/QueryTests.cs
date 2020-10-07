using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{


    [TestClass]
    public class QueryTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // All these tests are offline by design!
        }

        #region Helper methods

        private Tuple<TModel, EntityInfo> BuildModel<TModel, TModelInterface>(Expression<Func<TModelInterface, object>>[] expression = null, bool graphFirst = true) where TModel : new()
        {
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.GraphFirst = graphFirst;

                var model = new TModel();
                (model as IDataModelWithContext).PnPContext = context;

                var entityInfo = EntityManager.GetClassInfo(model.GetType(), (model as BaseDataModel<TModelInterface>), expression);

                return new Tuple<TModel, EntityInfo>(model, entityInfo);
            }
        }

        private async Task<string> GetAPICallTestAsync<TModel, TModelInterface>(Tuple<TModel, EntityInfo> input)
        {
            var apiCallRequest = await new QueryClient().BuildGetAPICallAsync(input.Item1 as BaseDataModel<TModelInterface>, input.Item2, default);
            return ReverseTokenParsing((input.Item1 as IDataModelWithContext).PnPContext, apiCallRequest.ApiCall.Request);
        }

        private static string ReverseTokenParsing(PnPContext context, string query)
        {
            query = query.Replace($"/{context.Uri.DnsSafeHost}", "/{hostname}");
            query = query.Replace($"{context.Uri.AbsolutePath}", "{serverrelativepath}");

            if (query.IndexOf("/_api/", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                return query.Substring(query.IndexOf("/_api/", StringComparison.InvariantCultureIgnoreCase) + 1);
            }

            return query;
        }

        #endregion

        /*
        [TestMethod]
        public async Task Samples()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title })),
                            "sites/{hostname}:{serverrelativepath}?$select=sharepointIds%2cdisplayName%2cid", true);

            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title }, graphFirst: false)),
                            "_api/web?$select=Id%2cTitle", true);

            Assert.AreEqual(await GetAPICallTestAsync<List, IList>(BuildModel<List, IList>(new Expression<Func<IList, object>>[] { p => p.Title })),
                            "sites/{Parent.GraphId}/lists/{GraphId}?$select=id%2cdisplayName", true);
        }
        */

        #region Basic GET tests using Web 

        [TestMethod]
        public async Task GetWebGraphFirstDefault()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>()),
                            "sites/{hostname}:{serverrelativepath}", true);
        }

        [TestMethod]
        public async Task GetWebDefault()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(graphFirst: false)),
                            "_api/web", true);
        }

        [TestMethod]
        public async Task GetWebGraphFirstExpressionSingleSimpleProperty()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title })),
                            "sites/{hostname}:{serverrelativepath}?$select=sharepointIds%2cdisplayName%2cid", true);
        }

        [TestMethod]
        public async Task GetWebExpressionSingleSimpleProperty()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title }, graphFirst: false)),
                            "_api/web?$select=Id%2cTitle", true);
        }

        [TestMethod]
        public async Task GetWebGraphFirstExpressionMultipleSimpleProperty()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title, p=>p.Description })),
                            "sites/{hostname}:{serverrelativepath}?$select=sharepointIds%2cdisplayName%2cdescription%2cid", true);
        }

        [TestMethod]
        public async Task GetWebExpressionMultipleSimpleProperty()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title, p => p.Description }, graphFirst: false)),
                            "_api/web?$select=Id%2cTitle%2cDescription", true);
        }

        [TestMethod]
        public async Task GetWebGraphFirstExpressionKeyProperty()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Id })),
                            "sites/{hostname}:{serverrelativepath}?$select=sharepointIds%2cid", true);
        }

        [TestMethod]
        public async Task GetWebExpressionKeyProperty()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Id }, graphFirst: false)),
                            "_api/web?$select=Id", true);
        }

        [TestMethod]
        public async Task GetWebGraphFirstExpressionKeyPlusSimpleProperties()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title, p => p.Description, p => p.Id })),
                            "sites/{hostname}:{serverrelativepath}?$select=sharepointIds%2cdisplayName%2cdescription%2cid", true);
        }

        [TestMethod]
        public async Task GetWebExpressionKeyPlusSimpleProperties()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title, p => p.Description, p => p.Id }, graphFirst: false)),
                            "_api/web?$select=Id%2cTitle%2cDescription", true);
        }

        [TestMethod]
        public async Task GetWebGraphFirstExpressionExpandableProperty()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Lists })),
                            "sites/{hostname}:{serverrelativepath}?$select=sharepointIds%2clists%2cid", true);
        }

        [TestMethod]
        public async Task GetWebExpressionExpandableProperty()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Lists }, graphFirst: false)),
                            "_api/web?$select=Id%2cLists&$expand=Lists", true);
        }

        [TestMethod]
        public async Task GetWebGraphFirstExpressionExpandablePlusSimpleProperties()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title, p => p.Description, p => p.Lists })),
                            "sites/{hostname}:{serverrelativepath}?$select=sharepointIds%2cdisplayName%2cdescription%2clists%2cid", true);
        }

        [TestMethod]
        public async Task GetWebExpressionExpandablePlusSimpleProperties()
        {
            Assert.AreEqual(await GetAPICallTestAsync<Web, IWeb>(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title, p => p.Description, p => p.Lists }, graphFirst: false)),
                            "_api/web?$select=Id%2cTitle%2cDescription%2cLists&$expand=Lists", true);
        }

        #endregion

    }
}
