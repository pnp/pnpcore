using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// RecycleBinItem class, write your custom code here
    /// </summary>
    [SharePointType("SP.RecycleBinItem", Target = typeof(Web),  
        Uri = "_api/Web/RecycleBin(guid'{Id}')", Get = "_api/Web/RecycleBin", LinqGet = "_api/Web/RecycleBin")]
    [SharePointType("SP.RecycleBinItem", Target = typeof(Site),
        Uri = "_api/Site/RecycleBin(guid'{Id}')", Get = "_api/Site/RecycleBin", LinqGet = "_api/Site/RecycleBin")]
    internal partial class RecycleBinItem
    {
        public RecycleBinItem()
        {
            MappingHandler = (FromJson input) =>
            {
                // implement custom mapping logic
                switch (input.TargetType.Name)
                {
                    case nameof(RecycleBinItemType): return JsonMappingHelper.ToEnum<RecycleBinItemType>(input.JsonElement);
                    case nameof(RecycleBinItemState): return JsonMappingHelper.ToEnum<RecycleBinItemState>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };
        }


        #region Methods

        #region Restore
        public async Task RestoreAsync()
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);
            string restoreEndpointUrl = $"{entity.SharePointUri}/Restore()";

            var apiCall = new ApiCall(restoreEndpointUrl, ApiType.SPORest);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void Restore()
        {
            RestoreAsync().GetAwaiter().GetResult();
        }

        public async Task RestoreBatchAsync(Batch batch)
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);
            string restoreEndpointUrl = $"{entity.SharePointUri}/Restore()";

            var apiCall = new ApiCall(restoreEndpointUrl, ApiType.SPORest);

            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void RestoreBatch(Batch batch)
        {
            RestoreBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task RestoreBatchAsync()
        {
            await RestoreBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void RestoreBatch()
        {
            RestoreBatchAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region MoveToSecondStage
        public async Task MoveToSecondStageAsync()
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);
            string restoreEndpointUrl = $"{entity.SharePointUri}/MoveToSecondStage()";

            var apiCall = new ApiCall(restoreEndpointUrl, ApiType.SPORest);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void MoveToSecondStage()
        {
            MoveToSecondStageAsync().GetAwaiter().GetResult();
        }

        public async Task MoveToSecondStageBatchAsync(Batch batch)
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);
            string restoreEndpointUrl = $"{entity.SharePointUri}/MoveToSecondStage()";

            var apiCall = new ApiCall(restoreEndpointUrl, ApiType.SPORest);

            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void MoveToSecondStageBatch(Batch batch)
        {
            MoveToSecondStageBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task MoveToSecondStageBatchAsync()
        {
           await MoveToSecondStageBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void MoveToSecondStageBatch()
        {
            MoveToSecondStageBatchAsync().GetAwaiter().GetResult();
        }
        #endregion

        #endregion
    }
}
