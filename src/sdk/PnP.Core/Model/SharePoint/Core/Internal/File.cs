using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// File class, write your custom code here
    /// </summary>
    [SharePointType("SP.File", Target = typeof(Folder), Uri = "_api/Web/getFileById('{Id}')", Get = "_api/Web/getFolderById('{Parent.Id}')/Files", LinqGet = "_api/Web/getFolderById('{Parent.Id}')/Files")]
    // TODO To implement when a token can be used to identify the parent list
    //[GraphType(Get = "sites/{hostname}:{serverrelativepath}/lists/{ParentList.Id}/items/{Id}")]
    internal partial class File
    {
        internal const string AddFileContentAdditionalInformationKey = "Content";
        internal const string AddFileOverwriteAdditionalInformationKey = "Overwrite";

        public File()
        {
            MappingHandler = (FromJson input) =>
            {
                // implement custom mapping logic
                switch (input.TargetType.Name)
                {
                    case nameof(CustomizedPageStatus): return JsonMappingHelper.ToEnum<CustomizedPageStatus>(input.JsonElement);
                    case nameof(PageRenderType): return JsonMappingHelper.ToEnum<ListPageRenderType>(input.JsonElement);
                    case nameof(CheckOutType): return JsonMappingHelper.ToEnum<CheckOutType>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };
        }

        #region Extensions
        #region Publish
        public async Task PublishAsync(string comment = null)
        {
            var entity = EntityManager.Instance.GetClassInfo(GetType(), this);
            string publishEndpointUrl = $"{entity.SharePointUri}/publish(comment='{comment ?? string.Empty}')";

            var apiCall = new ApiCall(publishEndpointUrl, ApiType.SPORest);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void Publish(string comment = null)
        {
            PublishAsync(comment).GetAwaiter().GetResult();
        }

        public async Task PublishBatchAsync(string comment = null)
        {
            await PublishBatchAsync(PnPContext.CurrentBatch, comment).ConfigureAwait(false);
        }

        public void PublishBatch(string comment = null)
        {
            PublishBatchAsync(comment).GetAwaiter().GetResult();
        }

        public async Task PublishBatchAsync(Batch batch, string comment = null)
        {
            var entity = EntityManager.Instance.GetClassInfo(GetType(), this);
            string publishEndpointUrl = $"{entity.SharePointUri}/publish(comment='{comment ?? string.Empty}')";

            var apiCall = new ApiCall(publishEndpointUrl, ApiType.SPORest);

            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void PublishBatch(Batch batch, string comment = null)
        {
            PublishBatchAsync(batch, comment).GetAwaiter().GetResult();
        }
        #endregion

        #region Unpublish
        public void Unpublish(string comment = null)
        {
            UnpublishAsync(comment).GetAwaiter().GetResult();
        }

        public async Task UnpublishAsync(string comment = null)
        {
            var entity = EntityManager.Instance.GetClassInfo(GetType(), this);
            string publishEndpointUrl = $"{entity.SharePointUri}/unpublish(comment='{comment ?? string.Empty}')";

            var apiCall = new ApiCall(publishEndpointUrl, ApiType.SPORest);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void UnpublishBatch(Batch batch, string comment = null)
        {
            UnpublishBatchAsync(batch, comment).GetAwaiter().GetResult();
        }

        public async Task UnpublishBatchAsync(string comment = null)
        {
            await UnpublishBatchAsync(PnPContext.CurrentBatch, comment).ConfigureAwait(false);
        }

        public async Task UnpublishBatchAsync(Batch batch, string comment = null)
        {
            var entity = EntityManager.Instance.GetClassInfo(GetType(), this);
            string publishEndpointUrl = $"{entity.SharePointUri}/unpublish(comment='{comment ?? string.Empty}')";

            var apiCall = new ApiCall(publishEndpointUrl, ApiType.SPORest);

            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void UnpublishBatch(string comment = null)
        {
            UnpublishBatchAsync(comment).GetAwaiter().GetResult();
        }
        #endregion

        #region Checkout
        public async Task CheckoutAsync()
        {
            var entity = EntityManager.Instance.GetClassInfo(GetType(), this);
            string checkoutEndpointUrl = $"{entity.SharePointUri}/checkout";

            var apiCall = new ApiCall(checkoutEndpointUrl, ApiType.SPORest);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void Checkout()
        {
            CheckoutAsync().GetAwaiter().GetResult();
        }

        public async Task CheckoutBatchAsync()
        {
            await CheckoutBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void CheckoutBatch()
        {
            CheckoutBatchAsync().GetAwaiter().GetResult();
        }

        public async Task CheckoutBatchAsync(Batch batch)
        {
            var entity = EntityManager.Instance.GetClassInfo(GetType(), this);
            string checkoutEndpointUrl = $"{entity.SharePointUri}/checkout";

            var apiCall = new ApiCall(checkoutEndpointUrl, ApiType.SPORest);

            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void CheckoutBatch(Batch batch)
        {
            CheckoutBatchAsync(batch).GetAwaiter().GetResult();
        }
        #endregion

        #region UndoCheckout
        public void UndoCheckout()
        {
            UndoCheckoutAsync().GetAwaiter().GetResult();
        }

        public async Task UndoCheckoutAsync()
        {
            var entity = EntityManager.Instance.GetClassInfo(GetType(), this);
            string undoCheckoutEndpointUrl = $"{entity.SharePointUri}/undoCheckout";

            var apiCall = new ApiCall(undoCheckoutEndpointUrl, ApiType.SPORest);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void UndoCheckoutBatch()
        {
            UndoCheckoutBatchAsync().GetAwaiter().GetResult();
        }

        public void UndoCheckoutBatch(Batch batch)
        {
            UndoCheckoutBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task UndoCheckoutBatchAsync()
        {
            await UndoCheckoutBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public async Task UndoCheckoutBatchAsync(Batch batch)
        {
            var entity = EntityManager.Instance.GetClassInfo(GetType(), this);
            string undoCheckoutEndpointUrl = $"{entity.SharePointUri}/undoCheckout";

            var apiCall = new ApiCall(undoCheckoutEndpointUrl, ApiType.SPORest);

            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }
        #endregion

        #region Checkin

        public async Task CheckinAsync(string comment = null, CheckinType checkinType = CheckinType.MinorCheckIn)
        {
            var entity = EntityManager.Instance.GetClassInfo(GetType(), this);
            string checkinEndpointUrl = $"{entity.SharePointUri}/checkin(comment='{comment ?? string.Empty}',checkintype={(int)checkinType})";

            var apiCall = new ApiCall(checkinEndpointUrl, ApiType.SPORest);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void Checkin(string comment = null, CheckinType checkinType = CheckinType.MinorCheckIn)
        {
            CheckinAsync(comment, checkinType).GetAwaiter().GetResult();
        }

        public void CheckinBatch(string comment = null, CheckinType checkinType = CheckinType.MinorCheckIn)
        {
            CheckinBatchAsync(comment, checkinType).GetAwaiter().GetResult();
        }

        public void CheckinBatch(Batch batch, string comment = null, CheckinType checkinType = CheckinType.MinorCheckIn)
        {
            CheckinBatchAsync(batch, comment, checkinType).GetAwaiter().GetResult();
        }

        public async Task CheckinBatchAsync(string comment = null, CheckinType checkinType = CheckinType.MinorCheckIn)
        {
            await CheckinBatchAsync(PnPContext.CurrentBatch, comment, checkinType).ConfigureAwait(false);
        }

        public async Task CheckinBatchAsync(Batch batch, string comment = null, CheckinType checkinType = CheckinType.MinorCheckIn)
        {
            var entity = EntityManager.Instance.GetClassInfo(GetType(), this);
            string checkinEndpointUrl = $"{entity.SharePointUri}/checkin(comment='{comment ?? string.Empty}',checkintype={(int)checkinType})";

            var apiCall = new ApiCall(checkinEndpointUrl, ApiType.SPORest);

            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }
        #endregion

        #region Recycle

        public Guid Recycle()
        {
            return RecycleAsync().GetAwaiter().GetResult();
        }

        public async Task<Guid> RecycleAsync()
        {
            var entity = EntityManager.Instance.GetClassInfo(GetType(), this);
            string recycleEndpointUrl = $"{entity.SharePointUri}/recycle";

            var apiCall = new ApiCall(recycleEndpointUrl, ApiType.SPORest);

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                var document = JsonSerializer.Deserialize<JsonElement>(response.Json);
                if (document.TryGetProperty("d", out JsonElement root))
                {
                    if (root.TryGetProperty("Recycle", out JsonElement recycleBinItemId))
                    {
                        // Remove this item from the lists collection
                        RemoveFromParentCollection();

                        // return the recyclebin item id
                        return recycleBinItemId.GetGuid();
                    }
                }
            }

            return Guid.Empty;
        }

        public void RecycleBatch()
        {
            RecycleBatchAsync().GetAwaiter().GetResult();
        }

        public void RecycleBatch(Batch batch)
        {
            RecycleBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task RecycleBatchAsync()
        {
            await RecycleBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public async Task RecycleBatchAsync(Batch batch)
        {
            var entity = EntityManager.Instance.GetClassInfo(GetType(), this);
            string recycleEndpointUrl = $"{entity.SharePointUri}/recycle";

            var apiCall = new ApiCall(recycleEndpointUrl, ApiType.SPORest);

            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }
        #endregion

        #region CopyTo
        private ApiCall GetCopyToSameSiteApiCall(string destinationUrl, bool overwrite)
        {
            var entityInfo = EntityManager.Instance.GetClassInfo(GetType(), this);
            // NOTE WebUtility encode spaces to "+" instead of %20
            string encodedDestinationUrl = WebUtility.UrlEncode(destinationUrl).Replace("+", "%20").Replace("/", "%2F");
            string copyToEndpointUrl = $"{entityInfo.SharePointUri}/copyTo(strnewurl='{encodedDestinationUrl}', boverwrite={overwrite.ToString().ToLower()})";

            return new ApiCall(copyToEndpointUrl, ApiType.SPORest);
        }

        private ApiCall GetCopyToCrossSiteApiCall(string destinationUrl, bool overwrite, MoveCopyOptions options)
        {
            string destUrl = UrlUtility.EnsureAbsoluteUrl(PnPContext.Uri, destinationUrl).ToString();
            string srcUrl = UrlUtility.EnsureAbsoluteUrl(PnPContext.Uri, ServerRelativeUrl).ToString();

            // Default is "overwrite=false" => "KeepBoth=true"
            options ??= new MoveCopyOptions() { KeepBoth = true};

            // If options are specified, use the CopyByPath API endpoint
            var parameters = new
            {
                destPath = new
                {
                    __metadata = new { type = "SP.ResourcePath" },
                    DecodedUrl = destUrl
                },
                srcPath = new
                {
                    __metadata = new { type = "SP.ResourcePath" },
                    DecodedUrl = srcUrl
                },
                options = new
                {
                    __metadata = new { type = "SP.MoveCopyOptions" },
                    options.KeepBoth,
                    options.ResetAuthorAndCreatedOnCopy,
                    options.ShouldBypassSharedLocks
                }
            }.AsExpando();
            string body = JsonSerializer.Serialize(parameters, typeof(ExpandoObject));
            string copyToEndpointUrl = $"_api/SP.MoveCopyUtil.CopyFileByPath(overwrite=@a1)?@a1={overwrite.ToString().ToLower()}";

            return new ApiCall(copyToEndpointUrl, ApiType.SPORest, body);
        }

        private ApiCall GetCopyToApiCall(string destinationUrl, bool overwrite, MoveCopyOptions options)
        {
            // If same site
            if (UrlUtility.IsSameSite(PnPContext.Uri, destinationUrl))
            {
                return GetCopyToSameSiteApiCall(destinationUrl, overwrite);
            }
            else
            {
                options ??= new MoveCopyOptions() { KeepBoth = !overwrite };
                return GetCopyToCrossSiteApiCall(destinationUrl, overwrite, options);
            }
        }

        public async Task CopyToAsync(string destinationUrl, bool overwrite = false, MoveCopyOptions options = null)
        {
            var apiCall = GetCopyToApiCall(destinationUrl, overwrite, options);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void CopyTo(string destinationUrl, bool overwrite = false, MoveCopyOptions options = null)
        {
            CopyToAsync(destinationUrl, overwrite).GetAwaiter().GetResult();
        }

        public async Task CopyToBatchAsync(Batch batch, string destinationUrl, bool overwrite = false, MoveCopyOptions options = null)
        {
            var apiCall = GetCopyToApiCall(destinationUrl, overwrite, options);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void CopyToBatch(Batch batch, string destinationUrl, bool overwrite = false, MoveCopyOptions options = null)
        {
            CopyToBatchAsync(batch, destinationUrl, overwrite, options).GetAwaiter().GetResult();
        }

        public async Task CopyToBatchAsync(string destinationUrl, bool overwrite = false, MoveCopyOptions options = null)
        {
            await CopyToBatchAsync(PnPContext.CurrentBatch, destinationUrl, overwrite, options).ConfigureAwait(false);
        }

        public void CopyToBatch(string destinationUrl, bool overwrite = false, MoveCopyOptions options = null)
        {
            CopyToBatchAsync(destinationUrl, overwrite, options).GetAwaiter().GetResult();
        }
        #endregion

        #region MoveTo
        private ApiCall GetMoveToSameSiteApiCall(string destinationUrl, MoveOperations moveOperations)
        {
            var entityInfo = EntityManager.Instance.GetClassInfo(GetType(), this);
            // NOTE WebUtility encode spaces to "+" instead of %20
            string encodedDestinationUrl = WebUtility.UrlEncode(destinationUrl).Replace("+", "%20").Replace("/", "%2F");
            string moveToEndpointUrl = $"{entityInfo.SharePointUri}/moveTo(newurl='{encodedDestinationUrl}', flags={(int)moveOperations})";

            return new ApiCall(moveToEndpointUrl, ApiType.SPORest);
        }

        private ApiCall GetMoveToCrossSiteApiCall(string destinationUrl, bool overwrite, MoveCopyOptions options)
        {
            string destUrl = UrlUtility.EnsureAbsoluteUrl(PnPContext.Uri, destinationUrl).ToString();
            string srcUrl = UrlUtility.EnsureAbsoluteUrl(PnPContext.Uri, ServerRelativeUrl).ToString();

            // Default is "overwrite=false" => "KeepBoth=true"
            options ??= new MoveCopyOptions() { KeepBoth = true };

            // If options are specified, use the CopyByPath API endpoint
            var parameters = new
            {
                destPath = new
                {
                    __metadata = new { type = "SP.ResourcePath" },
                    DecodedUrl = destUrl
                },
                srcPath = new
                {
                    __metadata = new { type = "SP.ResourcePath" },
                    DecodedUrl = srcUrl
                },
                options = new
                {
                    __metadata = new { type = "SP.MoveCopyOptions" },
                    options.KeepBoth,
                    options.ResetAuthorAndCreatedOnCopy,
                    options.ShouldBypassSharedLocks
                }
            }.AsExpando();
            string body = JsonSerializer.Serialize(parameters, typeof(ExpandoObject));
            string moveToEndpointUrl = $"_api/SP.MoveCopyUtil.MoveFileByPath(overwrite=@a1)?@a1={overwrite.ToString().ToLower()}";

            return new ApiCall(moveToEndpointUrl, ApiType.SPORest, body);
        }

        private ApiCall GetMoveToApiCall(string destinationUrl, MoveOperations moveOperations, MoveCopyOptions options)
        {
            // If same site
            if (UrlUtility.IsSameSite(PnPContext.Uri, destinationUrl))
            {
                return GetMoveToSameSiteApiCall(destinationUrl, moveOperations);
            }
            else
            {
                bool overwrite = moveOperations.HasFlag(MoveOperations.Overwrite);
                options ??= new MoveCopyOptions() { KeepBoth = !overwrite };
                return GetMoveToCrossSiteApiCall(destinationUrl, overwrite, options);
            }
        }

        public async Task MoveToAsync(string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null)
        {
            var apiCall = GetMoveToApiCall(destinationUrl, moveOperations, options);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void MoveTo(string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null)
        {
            MoveToAsync(destinationUrl, moveOperations, options).GetAwaiter().GetResult();
        }

        public async Task MoveToBatchAsync(Batch batch, string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null)
        {
            var apiCall = GetMoveToApiCall(destinationUrl, moveOperations, options);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void MoveToBatch(Batch batch, string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null)
        {
            MoveToBatchAsync(batch, destinationUrl, moveOperations, options).GetAwaiter().GetResult();
        }

        public async Task MoveToBatchAsync(string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null)
        {
            await MoveToBatchAsync(PnPContext.CurrentBatch, destinationUrl, moveOperations, options).ConfigureAwait(false);
        }

        public void MoveToBatch(string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null)
        {
            MoveToBatchAsync(destinationUrl, moveOperations, options).GetAwaiter().GetResult();
        }
        #endregion

        #endregion
    }
}
