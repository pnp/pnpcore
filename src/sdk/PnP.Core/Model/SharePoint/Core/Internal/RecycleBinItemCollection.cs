using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class RecycleBinItemCollection
    {
        #region DeleteAll
        public async Task DeleteAllAsync()
        {
            var item = new RecycleBinItem()
            {
                PnPContext = PnPContext,
                Parent = Parent
            };
            var entity = EntityManager.GetClassInfo(item.GetType(), item);
            string deleteAllEndpointUrl = $"{entity.SharePointGet}/DeleteAll()";

            var apiCall = new ApiCall(deleteAllEndpointUrl, ApiType.SPORest);

            // TODO is in the end calling a "raw request" but does not seem very natural to use this object for that? Might need some review
            await item.RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void DeleteAll()
        {
            DeleteAllAsync().GetAwaiter().GetResult();
        }

        public async Task DeleteAllBatchAsync(Batch batch)
        {
            var item = new RecycleBinItem()
            {
                PnPContext = PnPContext,
                Parent = Parent
            };
            var entity = EntityManager.GetClassInfo(item.GetType(), item);
            string deleteAllEndpointUrl = $"{entity.SharePointGet}/DeleteAll()";

            var apiCall = new ApiCall(deleteAllEndpointUrl, ApiType.SPORest);

            // TODO is in the end calling a "raw request" but does not seem very natural to use this object for that? Might need some review
            await item.RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void DeleteAllBatch(Batch batch)
        {
            DeleteAllBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task DeleteAllBatchAsync()
        {
            await DeleteAllBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void DeleteAllBatch()
        {
            DeleteAllBatchAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region DeleteAllSecondStageItems
        public async Task DeleteAllSecondStageItemsAsync()
        {
            var item = new RecycleBinItem()
            {
                PnPContext = PnPContext,
                Parent = Parent
            };
            var entity = EntityManager.GetClassInfo(item.GetType(), item);
            string deleteAllEndpointUrl = $"{entity.SharePointGet}/DeleteAllSecondStageItems()";

            var apiCall = new ApiCall(deleteAllEndpointUrl, ApiType.SPORest);

            // TODO is in the end calling a "raw request" but does not seem very natural to use this object for that? Might need some review
            await item.RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void DeleteAllSecondStageItems()
        {
            DeleteAllSecondStageItemsAsync().GetAwaiter().GetResult();
        }

        public async Task DeleteAllSecondStageItemsBatchAsync(Batch batch)
        {
            var item = new RecycleBinItem()
            {
                PnPContext = PnPContext,
                Parent = Parent
            };
            var entity = EntityManager.GetClassInfo(item.GetType(), item);
            string deleteAllEndpointUrl = $"{entity.SharePointGet}/DeleteAllSecondStageItems()";

            var apiCall = new ApiCall(deleteAllEndpointUrl, ApiType.SPORest);

            // TODO is in the end calling a "raw request" but does not seem very natural to use this object for that? Might need some review
            await item.RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void DeleteAllSecondStageItemsBatch(Batch batch)
        {
            DeleteAllSecondStageItemsBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task DeleteAllSecondStageItemsBatchAsync()
        {
            await DeleteAllSecondStageItemsBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void DeleteAllSecondStageItemsBatch()
        {
            DeleteAllSecondStageItemsBatchAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region MoveAllToSecondStage
        public async Task MoveAllToSecondStageAsync()
        {
            var item = new RecycleBinItem()
            {
                PnPContext = PnPContext,
                Parent = Parent
            };
            var entity = EntityManager.GetClassInfo(item.GetType(), item);
            string deleteAllEndpointUrl = $"{entity.SharePointGet}/MoveAllToSecondStage()";

            var apiCall = new ApiCall(deleteAllEndpointUrl, ApiType.SPORest);

            // TODO is in the end calling a "raw request" but does not seem very natural to use this object for that? Might need some review
            await item.RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void MoveAllToSecondStage()
        {
            MoveAllToSecondStageAsync().GetAwaiter().GetResult();
        }

        public async Task MoveAllToSecondStageBatchAsync(Batch batch)
        {
            var item = new RecycleBinItem()
            {
                PnPContext = PnPContext,
                Parent = Parent
            };
            var entity = EntityManager.GetClassInfo(item.GetType(), item);
            string deleteAllEndpointUrl = $"{entity.SharePointGet}/MoveAllToSecondStage()";

            var apiCall = new ApiCall(deleteAllEndpointUrl, ApiType.SPORest);

            // TODO is in the end calling a "raw request" but does not seem very natural to use this object for that? Might need some review
            await item.RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void MoveAllToSecondStageBatch(Batch batch)
        {
            MoveAllToSecondStageBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task MoveAllToSecondStageBatchAsync()
        {
            await MoveAllToSecondStageBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void MoveAllToSecondStageBatch()
        {
            MoveAllToSecondStageBatchAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region RestoreAll
        public async Task RestoreAllAsync()
        {
            var item = new RecycleBinItem()
            {
                PnPContext = PnPContext,
                Parent = Parent
            };
            var entity = EntityManager.GetClassInfo(item.GetType(), item);
            string deleteAllEndpointUrl = $"{entity.SharePointGet}/RestoreAll()";

            var apiCall = new ApiCall(deleteAllEndpointUrl, ApiType.SPORest);

            // TODO is in the end calling a "raw request" but does not seem very natural to use this object for that? Might need some review
            await item.RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void RestoreAll()
        {
            RestoreAllAsync().GetAwaiter().GetResult();
        }

        public async Task RestoreAllBatchAsync(Batch batch)
        {
            var item = new RecycleBinItem()
            {
                PnPContext = PnPContext,
                Parent = Parent
            };
            var entity = EntityManager.GetClassInfo(item.GetType(), item);
            string deleteAllEndpointUrl = $"{entity.SharePointGet}/RestoreAll()";

            var apiCall = new ApiCall(deleteAllEndpointUrl, ApiType.SPORest);

            // TODO is in the end calling a "raw request" but does not seem very natural to use this object for that? Might need some review
            await item.RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void RestoreAllBatch(Batch batch)
        {
            RestoreAllBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task RestoreAllBatchAsync()
        {
            await RestoreAllBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void RestoreAllBatch()
        {
            RestoreAllBatchAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region GetById

        public IRecycleBinItem GetById(Guid id, params Expression<Func<IRecycleBinItem, object>>[] selectors)
        {
            return BaseDataModelExtensions.BaseLinqGet(this, l => l.Id == id, selectors);
        }

        public async Task<IRecycleBinItem> GetByIdAsync(Guid id, params Expression<Func<IRecycleBinItem, object>>[] selectors)
        {
            return await BaseDataModelExtensions.BaseLinqGetAsync(this, l => l.Id == id, selectors).ConfigureAwait(false);
        }

        #endregion
    }
}
