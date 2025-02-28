using PnP.Core.Services;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal class RecyclableBaseDataModel<T> : BaseDataModel<T>
    {
        public Guid Recycle()
        {
            return RecycleAsync().GetAwaiter().GetResult();
        }

        public async Task<Guid> RecycleAsync()
        {
            var apiCall = BuildRecycleApiCall();

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                return ProcessRecycleResponse(response.Json);
            }

            return Guid.Empty;
        }

        private static Guid ProcessRecycleResponse(string json)
        {
            var document = JsonSerializer.Deserialize<JsonElement>(json);
            if (document.TryGetProperty("value", out JsonElement recycleBinItemId))
            {
                // return the recyclebin item id
                return recycleBinItemId.GetGuid();
            }

            return Guid.Empty;
        }

        public IBatchSingleResult<BatchResultValue<Guid>> RecycleBatch()
        {
            return RecycleBatchAsync().GetAwaiter().GetResult();
        }

        public IBatchSingleResult<BatchResultValue<Guid>> RecycleBatch(Batch batch)
        {
            return RecycleBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<BatchResultValue<Guid>>> RecycleBatchAsync()
        {
            return await RecycleBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public async Task<IBatchSingleResult<BatchResultValue<Guid>>> RecycleBatchAsync(Batch batch)
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);
            string recycleEndpointUrl = $"{entity.SharePointUri}/recycle";

            var apiCall = new ApiCall(recycleEndpointUrl, ApiType.SPORest)
            {
                RemoveFromModel = true,
                RawSingleResult = new BatchResultValue<Guid>(Guid.Empty),
                RawResultsHandler = (json, apiCall) =>
                {
                    (apiCall.RawSingleResult as BatchResultValue<Guid>).Value = ProcessRecycleResponse(json);
                }
            };

            var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            return new BatchSingleResult<BatchResultValue<Guid>>(batch, batchRequest.Id, apiCall.RawSingleResult as BatchResultValue<Guid>);
        }
        
        protected virtual ApiCall BuildRecycleApiCall()
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);
            string recycleEndpointUrl = $"{entity.SharePointUri}/recycle";

            return new ApiCall(recycleEndpointUrl, ApiType.SPORest)
            {
                RemoveFromModel = true
            };
        }

    }
}