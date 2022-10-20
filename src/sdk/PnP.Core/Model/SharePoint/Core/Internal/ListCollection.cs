using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class ListCollection : QueryableDataModelCollection<IList>, IListCollection
    {
        public ListCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Add methods

        public async Task<IList> AddBatchAsync(string title, ListTemplateType templateType)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, title, templateType).ConfigureAwait(false);
        }

        public IList AddBatch(string title, ListTemplateType templateType)
        {
            return AddBatchAsync(title, templateType).GetAwaiter().GetResult();
        }

        public async Task<IList> AddBatchAsync(Batch batch, string title, ListTemplateType templateType)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (templateType == 0)
            {
                throw new ArgumentException(string.Format(PnPCoreResources.Exception_CannotBeZero, nameof(templateType)));
            }

            var newList = CreateNewAndAdd() as List;

            newList.Title = title;
            newList.TemplateType = templateType;

            return await newList.AddBatchAsync(batch).ConfigureAwait(false) as List;
        }

        public IList AddBatch(Batch batch, string title, ListTemplateType templateType)
        {
            return AddBatchAsync(batch, title, templateType).GetAwaiter().GetResult();
        }

        public async Task<IList> AddAsync(string title, ListTemplateType templateType)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (templateType == 0)
            {
                throw new ArgumentException(string.Format(PnPCoreResources.Exception_CannotBeZero, nameof(templateType)));
            }

            var newList = CreateNewAndAdd() as List;

            newList.Title = title;
            newList.TemplateType = templateType;

            return await newList.AddAsync().ConfigureAwait(false) as List;
        }

        public IList Add(string title, ListTemplateType templateType)
        {
            return AddAsync(title, templateType).GetAwaiter().GetResult();
        }

        #endregion

        #region GetByTitle methods

        public IList GetByTitle(string title, params Expression<Func<IList, object>>[] selectors)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            return GetByTitleAsync(title, selectors).GetAwaiter().GetResult();
        }

        public async Task<IList> GetByTitleAsync(string title, params Expression<Func<IList, object>>[] selectors)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            var tempList = new List()
            {
                PnPContext = PnPContext,
                Parent = this
            };

            var entityInfo = EntityManager.GetClassInfo(tempList.GetType(), tempList, null, selectors);
            var apiCallRequest = await QueryClient.BuildGetAPICallAsync(tempList, entityInfo, new ApiCall($"_api/web/lists/getbytitle('{title}')", ApiType.SPORest), true).ConfigureAwait(false);

            try
            {
                await tempList.RequestAsync(apiCallRequest.ApiCall, HttpMethod.Get).ConfigureAwait(false);
                return tempList;
            }
            catch(SharePointRestServiceException ex)
            {
                if (ex.Error is SharePointRestError error && error.HttpResponseCode == (int)HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IList> GetByTitleBatchAsync(Batch batch, string title, params Expression<Func<IList, object>>[] selectors)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            List list = new List()
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await list.BaseBatchRetrieveAsync(batch, apiOverride: BuildGetListByTitleApiCall(title),
                                                                    fromJsonCasting: list.MappingHandler,
                                                                    postMappingJson: list.PostMappingHandler,
                                                                    selectors: selectors).ConfigureAwait(false);
            return list;

        }

        public IList GetByTitleBatch(Batch batch, string title, params Expression<Func<IList, object>>[] selectors)
        {
            return GetByTitleBatchAsync(batch, title, selectors).GetAwaiter().GetResult();
        }

        public async Task<IList> GetByTitleBatchAsync(string title, params Expression<Func<IList, object>>[] selectors)
        {
            return await GetByTitleBatchAsync(PnPContext.CurrentBatch, title, selectors).ConfigureAwait(false);
        }

        public IList GetByTitleBatch(string title, params Expression<Func<IList, object>>[] selectors)
        {
            return GetByTitleBatchAsync(title, selectors).GetAwaiter().GetResult();
        }

        private static ApiCall BuildGetListByTitleApiCall(string listTitle)
        {
            return new ApiCall($"_api/web/lists/getbytitle('{listTitle}')", ApiType.SPORest);
        }
        #endregion

        #region GetById methods

        public IList GetById(Guid id, params Expression<Func<IList, object>>[] selectors)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetByIdAsync(id, selectors).GetAwaiter().GetResult();
        }

        public async Task<IList> GetByIdAsync(Guid id, params Expression<Func<IList, object>>[] selectors)
        {

            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await this.QueryProperties(selectors).FirstOrDefaultAsync(l => l.Id == id).ConfigureAwait(false);
        }

        public async Task<IList> GetByIdBatchAsync(Batch batch, Guid id, params Expression<Func<IList, object>>[] selectors)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            List list = new List()
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await list.BaseBatchRetrieveAsync(batch, apiOverride: BuildGetListByIdApiCall(id),
                                                                    fromJsonCasting: list.MappingHandler,
                                                                    postMappingJson: list.PostMappingHandler,
                                                                    selectors: selectors).ConfigureAwait(false);
            return list;

        }

        public IList GetByIdBatch(Batch batch, Guid id, params Expression<Func<IList, object>>[] selectors)
        {
            return GetByIdBatchAsync(batch, id, selectors).GetAwaiter().GetResult();
        }

        public async Task<IList> GetByIdBatchAsync(Guid id, params Expression<Func<IList, object>>[] selectors)
        {
            return await GetByIdBatchAsync(PnPContext.CurrentBatch, id, selectors).ConfigureAwait(false);
        }

        public IList GetByIdBatch(Guid id, params Expression<Func<IList, object>>[] selectors)
        {
            return GetByIdBatchAsync(id, selectors).GetAwaiter().GetResult();
        }

        private static ApiCall BuildGetListByIdApiCall(Guid id)
        {
            return new ApiCall($"_api/web/lists/getbyid('{id}')", ApiType.SPORest);
        }
        #endregion

        #region GetByServerRelativeUrl methods

        public async Task<IList> GetByServerRelativeUrlAsync(string serverRelativeUrl, params Expression<Func<IList, object>>[] selectors)
        {
            if (serverRelativeUrl == null)
            {
                throw new ArgumentNullException(nameof(serverRelativeUrl));
            }

            if (string.IsNullOrEmpty(serverRelativeUrl))
            {
                throw new ArgumentException(PnPCoreResources.Exception_GetListByServerRelativeUrl_ServerRelativeUrl);
            }

            return await BaseDataModelExtensions.BaseGetAsync(this, BuildGetListByServerRelativeUrlApiCall(serverRelativeUrl), selectors).ConfigureAwait(false);
        }

        public IList GetByServerRelativeUrl(string serverRelativeUrl, params Expression<Func<IList, object>>[] selectors)
        {
            if (serverRelativeUrl == null)
            {
                throw new ArgumentNullException(nameof(serverRelativeUrl));
            }

            return GetByServerRelativeUrlAsync(serverRelativeUrl, selectors).GetAwaiter().GetResult();
        }

        public async Task<IList> GetByServerRelativeUrlBatchAsync(string serverRelativeUrl, params Expression<Func<IList, object>>[] selectors)
        {
            if (serverRelativeUrl == null)
            {
                throw new ArgumentNullException(nameof(serverRelativeUrl));
            }

            if (string.IsNullOrEmpty(serverRelativeUrl))
            {
                throw new ArgumentException(PnPCoreResources.Exception_GetListByServerRelativeUrl_ServerRelativeUrl);
            }

            return await GetByServerRelativeUrlBatchAsync(PnPContext.CurrentBatch, serverRelativeUrl, selectors).ConfigureAwait(false);
        }

        public IList GetByServerRelativeUrlBatch(string serverRelativeUrl, params Expression<Func<IList, object>>[] selectors)
        {
            if (serverRelativeUrl == null)
            {
                throw new ArgumentNullException(nameof(serverRelativeUrl));
            }

            return GetByServerRelativeUrlBatchAsync(serverRelativeUrl, selectors).GetAwaiter().GetResult();
        }

        public async Task<IList> GetByServerRelativeUrlBatchAsync(Batch batch, string serverRelativeUrl, params Expression<Func<IList, object>>[] selectors)
        {
            if (serverRelativeUrl == null)
            {
                throw new ArgumentNullException(nameof(serverRelativeUrl));
            }

            if (string.IsNullOrEmpty(serverRelativeUrl))
            {
                throw new ArgumentException(PnPCoreResources.Exception_GetListByServerRelativeUrl_ServerRelativeUrl);
            }

            List list = new List()
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await list.BaseBatchRetrieveAsync(batch, apiOverride: BuildGetListByServerRelativeUrlApiCall(serverRelativeUrl),
                                                                    fromJsonCasting: list.MappingHandler,
                                                                    postMappingJson: list.PostMappingHandler,
                                                                    selectors: selectors).ConfigureAwait(false);
            return list;
        }

        public IList GetByServerRelativeUrlBatch(Batch batch, string serverRelativeUrl, params Expression<Func<IList, object>>[] selectors)
        {
            if (serverRelativeUrl == null)
            {
                throw new ArgumentNullException(nameof(serverRelativeUrl));
            }

            return GetByServerRelativeUrlBatchAsync(batch, serverRelativeUrl, selectors).GetAwaiter().GetResult();
        }


        private static ApiCall BuildGetListByServerRelativeUrlApiCall(string serverRelativeUrl)
        {
            return new ApiCall($"_api/web/getlist('{serverRelativeUrl}')", ApiType.SPORest);
        }

        #endregion

        #region EnsureSiteAssetsLibrary methods
        public async Task<IList> EnsureSiteAssetsLibraryAsync(params Expression<Func<IList, object>>[] selectors)
        {
            var assetLibrary = CreateNew() as List;

            var apiCall = BuildEnsureSiteAssetsLibraryApiCall();
            var entityInfo = EntityManager.GetClassInfo(assetLibrary.GetType(), assetLibrary, expressions: selectors);
            var query = await QueryClient.BuildGetAPICallAsync(assetLibrary, entityInfo, apiCall).ConfigureAwait(false);

            await assetLibrary.RequestAsync(new ApiCall(query.ApiCall.Request, ApiType.SPORest), System.Net.Http.HttpMethod.Post).ConfigureAwait(false);
            return assetLibrary;
        }

        public IList EnsureSiteAssetsLibrary(params Expression<Func<IList, object>>[] selectors)
        {
            return EnsureSiteAssetsLibraryAsync(selectors).GetAwaiter().GetResult();
        }

        public async Task<IList> EnsureSiteAssetsLibraryBatchAsync(params Expression<Func<IList, object>>[] selectors)
        {
            return await EnsureSiteAssetsLibraryBatchAsync(PnPContext.CurrentBatch, selectors).ConfigureAwait(false);
        }

        public IList EnsureSiteAssetsLibraryBatch(params Expression<Func<IList, object>>[] selectors)
        {
            return EnsureSiteAssetsLibraryBatchAsync(selectors).GetAwaiter().GetResult();
        }

        public async Task<IList> EnsureSiteAssetsLibraryBatchAsync(Batch batch, params Expression<Func<IList, object>>[] selectors)
        {
            var assetLibrary = CreateNew() as List;

            var apiCall = BuildEnsureSiteAssetsLibraryApiCall();
            var entityInfo = EntityManager.GetClassInfo(assetLibrary.GetType(), assetLibrary, expressions: selectors);
            var query = await QueryClient.BuildGetAPICallAsync(assetLibrary, entityInfo, apiCall).ConfigureAwait(false);

            await assetLibrary.RequestBatchAsync(batch, new ApiCall(query.ApiCall.Request, ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);

            return assetLibrary;
        }

        public IList EnsureSiteAssetsLibraryBatch(Batch batch, params Expression<Func<IList, object>>[] selectors)
        {
            return EnsureSiteAssetsLibraryBatchAsync(batch, selectors).GetAwaiter().GetResult();
        }

        private static ApiCall BuildEnsureSiteAssetsLibraryApiCall()
        {
            return new ApiCall("_api/Web/Lists/EnsureSiteAssetsLibrary", ApiType.SPORest);
        }
        #endregion
    }
}
