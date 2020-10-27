using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
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
    internal partial class RecycleBinItem : BaseDataModel<IRecycleBinItem>, IRecycleBinItem
    {
        #region Construction
        public RecycleBinItem()
        {
        }
        #endregion

        #region Properties
        public string AuthorEmail { get => GetValue<string>(); set => SetValue(value); }

        public string AuthorName { get => GetValue<string>(); set => SetValue(value); }

        public string DeletedByEmail { get => GetValue<string>(); set => SetValue(value); }

        public string DeletedByName { get => GetValue<string>(); set => SetValue(value); }

        public DateTime DeletedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public string DeletedDateLocalFormatted { get => GetValue<string>(); set => SetValue(value); }

        public string DirName { get => GetValue<string>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public RecycleBinItemState ItemState { get => GetValue<RecycleBinItemState>(); set => SetValue(value); }

        public RecycleBinItemType ItemType { get => GetValue<RecycleBinItemType>(); set => SetValue(value); }

        public string LeafName { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public long Size { get => GetValue<long>(); set => SetValue(value); }

        public ISharePointUser Author { get => GetModelValue<ISharePointUser>(); }

        public ISharePointUser DeletedBy { get => GetModelValue<ISharePointUser>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
        #endregion

        #region Extension methods

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
