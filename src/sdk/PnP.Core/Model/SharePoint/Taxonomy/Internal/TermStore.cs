using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [GraphType(Uri = "sites/{hostname},{Site.Id},{Web.Id}/termstore")]
    internal sealed class TermStore : BaseDataModel<ITermStore>, ITermStore
    {
        #region Properties
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("defaultLanguageTag")]
        public string DefaultLanguage { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("languageTags")]
        public List<string> Languages { get => GetValue<List<string>>(); set => SetValue(value); }

        [GraphProperty("groups", Get = "sites/{hostname},{Site.Id},{Web.Id}/termstore/groups")]
        public ITermGroupCollection Groups { get => GetModelCollectionValue<ITermGroupCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion

        #region Methods

        public async Task<ITermSet> GetTermSetByIdAsync(string id, params Expression<Func<ITermSet, object>>[] selectors)
        {
            var termSet = new TermSet()
            {
                PnPContext = PnPContext
            };

            var apiCall = BuildGetTermSetByIdApiCall(id);

            var entityInfo = EntityManager.GetClassInfo(termSet.GetType(), termSet, expressions: selectors);
            var query = await QueryClient.BuildGetAPICallAsync(termSet, entityInfo, apiCall).ConfigureAwait(false);

            await termSet.RequestAsync(new ApiCall(query.ApiCall.Request, ApiType.Graph), HttpMethod.Get).ConfigureAwait(false);

            return termSet;
        }

        public ITermSet GetTermSetById(string id, params Expression<Func<ITermSet, object>>[] selectors)
        {
            return GetTermSetByIdAsync(id, selectors).GetAwaiter().GetResult();
        }

        public async Task<ITermSet> GetTermSetByIdBatchAsync(string id, params Expression<Func<ITermSet, object>>[] selectors)
        {
            return await GetTermSetByIdBatchAsync(PnPContext.CurrentBatch, id, selectors).ConfigureAwait(false);
        }

        public ITermSet GetTermSetByIdBatch(string id, params Expression<Func<ITermSet, object>>[] selectors)
        {
            return GetTermSetByIdBatchAsync(id, selectors).GetAwaiter().GetResult();
        }

        public async Task<ITermSet> GetTermSetByIdBatchAsync(Batch batch, string id, params Expression<Func<ITermSet, object>>[] selectors)
        {
            var termSet = new TermSet()
            {
                PnPContext = PnPContext
            };

            var apiCall = BuildGetTermSetByIdApiCall(id);

            var entityInfo = EntityManager.GetClassInfo(termSet.GetType(), termSet, expressions: selectors);
            var query = await QueryClient.BuildGetAPICallAsync(termSet, entityInfo, apiCall).ConfigureAwait(false);

            await termSet.RequestBatchAsync(batch, new ApiCall(query.ApiCall.Request, ApiType.Graph), HttpMethod.Get).ConfigureAwait(false);

            return termSet;
        }

        public ITermSet GetTermSetByIdBatch(Batch batch, string id, params Expression<Func<ITermSet, object>>[] selectors)
        {
            return GetTermSetByIdBatchAsync(batch, id, selectors).GetAwaiter().GetResult();
        }

        private ApiCall BuildGetTermSetByIdApiCall(string termSetId)
        {
            return new ApiCall($"sites/{PnPContext.Site.Id}/termstore/sets/{termSetId}", ApiType.Graph);
        }

        public async Task<ITerm> GetTermByIdAsync(string termSetId, string termId, params Expression<Func<ITerm, object>>[] selectors)
        {
            var term = new Term()
            {
                PnPContext = PnPContext
            };

            var apiCall = BuildGetTermByIdApiCall(termSetId, termId);

            var entityInfo = EntityManager.GetClassInfo(term.GetType(), term, expressions: selectors);
            var query = await QueryClient.BuildGetAPICallAsync(term, entityInfo, apiCall).ConfigureAwait(false);

            await term.RequestAsync(new ApiCall(query.ApiCall.Request, ApiType.Graph), HttpMethod.Get).ConfigureAwait(false);

            return term;
        }

        public ITerm GetTermById(string termSetId, string termId, params Expression<Func<ITerm, object>>[] selectors)
        {
            return GetTermByIdAsync(termSetId, termId, selectors).GetAwaiter().GetResult();
        }

        public async Task<ITerm> GetTermByIdBatchAsync(string termSetId, string termId, params Expression<Func<ITerm, object>>[] selectors)
        {
            return await GetTermByIdBatchAsync(PnPContext.CurrentBatch, termSetId, termId, selectors).ConfigureAwait(false);
        }

        public ITerm GetTermByIdBatch(string termSetId, string termId, params Expression<Func<ITerm, object>>[] selectors)
        {
            return GetTermByIdBatchAsync(termSetId, termId, selectors).GetAwaiter().GetResult();
        }

        public async Task<ITerm> GetTermByIdBatchAsync(Batch batch, string termSetId, string termId, params Expression<Func<ITerm, object>>[] selectors)
        {
            var term = new Term()
            {
                PnPContext = PnPContext
            };

            var apiCall = BuildGetTermByIdApiCall(termSetId, termId);

            var entityInfo = EntityManager.GetClassInfo(term.GetType(), term, expressions: selectors);
            var query = await QueryClient.BuildGetAPICallAsync(term, entityInfo, apiCall).ConfigureAwait(false);

            await term.RequestBatchAsync(batch, new ApiCall(query.ApiCall.Request, ApiType.Graph), HttpMethod.Get).ConfigureAwait(false);

            return term;
        }

        public ITerm GetTermByIdBatch(Batch batch, string termSetId, string termId, params Expression<Func<ITerm, object>>[] selectors)
        {
            return GetTermByIdBatchAsync(batch, termSetId, termId, selectors).GetAwaiter().GetResult();
        }

        private ApiCall BuildGetTermByIdApiCall(string termSetId, string termId)
        {
            return new ApiCall($"sites/{PnPContext.Site.Id}/termstore/sets/{termSetId}/terms/{termId}", ApiType.Graph);
        }

        #endregion
    }
}