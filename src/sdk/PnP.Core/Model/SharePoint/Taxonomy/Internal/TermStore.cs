using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
            // Ensure the manadatory properties are requested
            Expression<Func<ITerm, object>>[] defaultTermProps = { s => s.Id, s => s.Labels, s => s.Descriptions, s => s.Set };
            IEnumerable<Expression<Func<ITerm, object>>> defaultTermPropsToUse = defaultTermProps;
            defaultTermPropsToUse = defaultTermProps.Union(selectors);

            var term = new Term()
            {
                PnPContext = PnPContext
            };

            var apiCall = BuildGetTermByIdApiCall(termSetId, termId);

            var entityInfo = EntityManager.GetClassInfo(term.GetType(), term, expressions: defaultTermPropsToUse.ToArray());
            var query = await QueryClient.BuildGetAPICallAsync(term, entityInfo, apiCall).ConfigureAwait(false);

            await term.RequestAsync(new ApiCall(query.ApiCall.Request, ApiType.Graph), HttpMethod.Get).ConfigureAwait(false);

            if (term is IMetadataExtensible metadataExtensible)
            {
                metadataExtensible.Metadata[PnPConstants.MetaDataTermSetId] = termSetId;
            }

            // Load the term parent so that we can build up the proper parent tree to allow future operations on this term
            var parentOfTerm = await term.GetParentAsync().ConfigureAwait(false);

            if (parentOfTerm == null)
            {
                // The term is a root term in the termset
                if (term.IsPropertyAvailable(p => p.Set))
                {
                    term.Parent = term.Set;
                }
            }
            else
            {
                // Populate the set in case we have it
                if (term.IsPropertyAvailable(p => p.Set))
                {
                    // Set key properties we need for minimal actions
                    parentOfTerm.Set.SetSystemProperty(p=>p.Id, term.Set.Id);

                    // Copy metadata from the parent to the term
                    (parentOfTerm.Set as TermSet).Metadata = new Dictionary<string, string>(term.Metadata);

                    // Mark requested
                    parentOfTerm.Set.Requested = true;

                    // Also set the termset as parent, needed to be able to resolve this termset id in all cases
                    parentOfTerm.Parent = term.Set;
                }

                // The term is a child term of another term
                var termCollection = new TermCollection(PnPContext, null)
                {
                    Parent = parentOfTerm
                };

                // Add the retrieved term to the term collection
                termCollection.Add(term);

                term.Parent = termCollection;
            }

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