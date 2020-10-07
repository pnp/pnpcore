using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
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

        private Tuple<TModel, EntityInfo, Expression<Func<TModelInterface, object>>[]> BuildModel<TModel, TModelInterface>(Expression<Func<TModelInterface, object>>[] expression = null, bool graphFirst = true) where TModel : new()
        {
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.GraphFirst = graphFirst;

                var model = new TModel();
                (model as IDataModelWithContext).PnPContext = context;

                var entityInfo = EntityManager.GetClassInfo(model.GetType(), (model as BaseDataModel<TModelInterface>), expression);

                return new Tuple<TModel, EntityInfo, Expression<Func<TModelInterface, object>>[]>(model, entityInfo, expression);
            }
        }

        private async Task<List<string>> GetAPICallTestAsync<TModel, TModelInterface>(Tuple<TModel, EntityInfo, Expression<Func<TModelInterface, object>>[]> input)
        {
            List<string> requests = new List<string>();

            // Run the basic query generation
            var apiCallRequest = await new QueryClient().BuildGetAPICallAsync(input.Item1 as BaseDataModel<TModelInterface>, input.Item2, default);
            requests.Add(CleanRequestUrl((input.Item1 as IDataModelWithContext).PnPContext, apiCallRequest.ApiCall.Request));

            // Run the extra query generation (used to handle non expandable queries via a single batch)
            if (apiCallRequest.ApiCall.Type == ApiType.Graph || apiCallRequest.ApiCall.Type == ApiType.GraphBeta)
            {
                var nonExpandableRequests = await GetNonExpandableTestAsync(input);
                requests.AddRange(nonExpandableRequests);
            }            

            return requests;
        }

        private async Task<List<string>> GetNonExpandableTestAsync<TModel, TModelInterface>(Tuple<TModel, EntityInfo, Expression<Func<TModelInterface, object>>[]> input)
        {
            var batch = (input.Item1 as IDataModelWithContext).PnPContext.NewBatch();

            await new QueryClient().AddGraphBatchRequestsForNonExpandableCollectionsAsync(input.Item1 as BaseDataModel<TModelInterface>, batch, input.Item2, input.Item3, null, null);

            List<string> requests = new List<string>();
            foreach(var request in batch.Requests)
            {
                requests.Add(CleanRequestUrl((input.Item1 as IDataModelWithContext).PnPContext, request.Value.ApiCall.Request));
            }

            return requests;
        }

        private static string CleanRequestUrl(PnPContext context, string query)
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

        #region Basic GET tests using Web 

        [TestMethod]
        public async Task GetWebGraphFirstDefault()
        {
            var requests = await GetAPICallTestAsync(BuildModel<Web, IWeb>());
            Assert.IsTrue(requests.Count == 1);
            Assert.AreEqual(requests[0], "sites/{hostname}:{serverrelativepath}", true);
        }

        [TestMethod]
        public async Task GetWebDefault()
        {
            var requests = await GetAPICallTestAsync(BuildModel<Web, IWeb>(graphFirst: false));
            Assert.IsTrue(requests.Count == 1);
            Assert.AreEqual(requests[0], "_api/web", true);
        }

        [TestMethod]
        public async Task GetWebGraphFirstExpressionSingleSimpleProperty()
        {
            var requests = await GetAPICallTestAsync(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title }));
            Assert.IsTrue(requests.Count == 1);
            Assert.AreEqual(requests[0], "sites/{hostname}:{serverrelativepath}?$select=sharepointIds%2cdisplayName%2cid", true);
        }

        [TestMethod]
        public async Task GetWebExpressionSingleSimpleProperty()
        {
            var requests = await GetAPICallTestAsync(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title }, graphFirst: false));
            Assert.IsTrue(requests.Count == 1);
            Assert.AreEqual(requests[0], "_api/web?$select=Id%2cTitle", true);
        }

        [TestMethod]
        public async Task GetWebGraphFirstExpressionMultipleSimpleProperty()
        {
            var requests = await GetAPICallTestAsync(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title, p => p.Description }));
            Assert.IsTrue(requests.Count == 1);
            Assert.AreEqual(requests[0], "sites/{hostname}:{serverrelativepath}?$select=sharepointIds%2cdisplayName%2cdescription%2cid", true);
        }

        [TestMethod]
        public async Task GetWebExpressionMultipleSimpleProperty()
        {
            var requests = await GetAPICallTestAsync(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title, p => p.Description }, graphFirst: false));
            Assert.IsTrue(requests.Count == 1);
            Assert.AreEqual(requests[0], "_api/web?$select=Id%2cTitle%2cDescription", true);
        }

        [TestMethod]
        public async Task GetWebGraphFirstExpressionKeyProperty()
        {
            var requests = await GetAPICallTestAsync(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Id }));
            Assert.IsTrue(requests.Count == 1);
            Assert.AreEqual(requests[0], "sites/{hostname}:{serverrelativepath}?$select=sharepointIds%2cid", true);
        }

        [TestMethod]
        public async Task GetWebExpressionKeyProperty()
        {
            var requests = await GetAPICallTestAsync(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Id }, graphFirst: false));
            Assert.IsTrue(requests.Count == 1);
            Assert.AreEqual(requests[0], "_api/web?$select=Id", true);
        }

        [TestMethod]
        public async Task GetWebGraphFirstExpressionKeyPlusSimpleProperties()
        {
            var requests = await GetAPICallTestAsync(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title, p => p.Description, p => p.Id }));
            Assert.IsTrue(requests.Count == 1);
            Assert.AreEqual(requests[0], "sites/{hostname}:{serverrelativepath}?$select=sharepointIds%2cdisplayName%2cdescription%2cid", true);
        }

        [TestMethod]
        public async Task GetWebExpressionKeyPlusSimpleProperties()
        {
            var requests = await GetAPICallTestAsync(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title, p => p.Description, p => p.Id }, graphFirst: false));
            Assert.IsTrue(requests.Count == 1);
            Assert.AreEqual(requests[0], "_api/web?$select=Id%2cTitle%2cDescription", true);
        }

        [TestMethod]
        public async Task GetWebGraphFirstExpressionExpandableProperty()
        {
            var requests = await GetAPICallTestAsync(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Lists }));
            Assert.IsTrue(requests.Count == 2);
            Assert.AreEqual(requests[0], "sites/{hostname}:{serverrelativepath}?$select=sharepointIds%2clists%2cid", true);
            Assert.AreEqual(requests[1], "sites/{hostname}:{serverrelativepath}:/lists?$select=system,createdDateTime,description,eTag,id,lastModifiedDateTime,name,webUrl,displayName,createdBy,lastModifiedBy,parentReference,list", true);            
        }

        [TestMethod]
        public async Task GetWebExpressionExpandableProperty()
        {
            var requests = await GetAPICallTestAsync(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Lists }, graphFirst: false));
            Assert.IsTrue(requests.Count == 1);
            Assert.AreEqual(requests[0], "_api/web?$select=Id%2cLists&$expand=Lists", true);
        }

        [TestMethod]
        public async Task GetWebGraphFirstExpressionExpandablePlusSimpleProperties()
        {
            var requests = await GetAPICallTestAsync(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title, p => p.Description, p => p.Lists }));
            Assert.IsTrue(requests.Count == 2);
            Assert.AreEqual(requests[0], "sites/{hostname}:{serverrelativepath}?$select=sharepointIds%2cdisplayName%2cdescription%2clists%2cid", true);
            Assert.AreEqual(requests[1], "sites/{hostname}:{serverrelativepath}:/lists?$select=system,createdDateTime,description,eTag,id,lastModifiedDateTime,name,webUrl,displayName,createdBy,lastModifiedBy,parentReference,list", true);
        }

        [TestMethod]
        public async Task GetWebExpressionExpandablePlusSimpleProperties()
        {
            var requests = await GetAPICallTestAsync(BuildModel<Web, IWeb>(new Expression<Func<IWeb, object>>[] { p => p.Title, p => p.Description, p => p.Lists }, graphFirst: false));
            Assert.IsTrue(requests.Count == 1);
            Assert.AreEqual(requests[0], "_api/web?$select=Id%2cTitle%2cDescription%2cLists&$expand=Lists", true);
        }

        #endregion

    }
}
