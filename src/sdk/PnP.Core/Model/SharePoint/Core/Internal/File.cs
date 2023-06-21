using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// File class, write your custom code here
    /// </summary>
    [SharePointType("SP.File", Target = typeof(Folder), Uri = "_api/Web/getFileById('{Id}')", LinqGet = "_api/Web/getFolderById('{Parent.Id}')/Files")]
    [SharePointType("SP.File", Target = typeof(Web), Uri = "_api/Web/getFileById('{Id}')")]
    [SharePointType("SP.File", Target = typeof(ListItem), Uri = "_api/Web/lists(guid'{List.Id}')/items({Parent.Id})/file")]

    internal sealed class File : BaseDataModel<IFile>, IFile
    {
        internal const string AddFileContentAdditionalInformationKey = "Content";
        internal const string AddFileOverwriteAdditionalInformationKey = "Overwrite";
        #region Construction
        public File()
        {
        }
        #endregion

        #region Properties
        public string CheckInComment { get => GetValue<string>(); set => SetValue(value); }

        public CheckOutType CheckOutType { get => GetValue<CheckOutType>(); set => SetValue(value); }

        public string ContentTag { get => GetValue<string>(); set => SetValue(value); }

        public CustomizedPageStatus CustomizedPageStatus { get => GetValue<CustomizedPageStatus>(); set => SetValue(value); }

        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ETag { get => GetValue<string>(); set => SetValue(value); }

        public bool HasAlternateContentStreams { get => GetValue<bool>(); set => SetValue(value); }

        public bool IrmEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public long Length { get => GetValue<long>(); set => SetValue(value); }

        public string LinkingUri { get => GetValue<string>(); set => SetValue(value); }

        public string LinkingUrl { get => GetValue<string>(); set => SetValue(value); }

        public int MajorVersion { get => GetValue<int>(); set => SetValue(value); }

        public int MinorVersion { get => GetValue<int>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public ListPageRenderType PageRenderType { get => GetValue<ListPageRenderType>(); set => SetValue(value); }

        public PublishedStatus Level { get => GetValue<PublishedStatus>(); set => SetValue(value); }

        public string ServerRedirectedUrl { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public Guid SiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public DateTime TimeCreated { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime TimeLastModified { get => GetValue<DateTime>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public int UIVersion { get => GetValue<int>(); set => SetValue(value); }

        public string UIVersionLabel { get => GetValue<string>(); set => SetValue(value); }

        public Guid UniqueId { get => GetValue<Guid>(); set => SetValue(value); }

        public string VroomDriveID { get => GetValue<string>(); set => SetValue(value); }

        public string VroomItemID { get => GetValue<string>(); set => SetValue(value); }

        public Guid WebId { get => GetValue<Guid>(); set => SetValue(value); }

        public IListItem ListItemAllFields { get => GetModelValue<IListItem>(); }

        public IEffectiveInformationRightsManagementSettings EffectiveInformationRightsManagementSettings { get => GetModelValue<IEffectiveInformationRightsManagementSettings>(); }

        public IInformationRightsManagementFileSettings InformationRightsManagementSettings { get => GetModelValue<IInformationRightsManagementFileSettings>(); }

        public IPropertyValues Properties { get => GetModelValue<IPropertyValues>(); }

        public IFileVersionEventCollection VersionEvents { get => GetModelCollectionValue<IFileVersionEventCollection>(); }

        public IFileVersionCollection Versions { get => GetModelCollectionValue<IFileVersionCollection>(); }

        public ISharePointUser Author { get => GetModelValue<ISharePointUser>(); }

        public ISharePointUser CheckedOutByUser { get => GetModelValue<ISharePointUser>(); }

        public ISharePointUser LockedByUser { get => GetModelValue<ISharePointUser>(); }

        public ISharePointUser ModifiedBy { get => GetModelValue<ISharePointUser>(); }

        [KeyProperty(nameof(UniqueId))]
        public override object Key { get => UniqueId; set => UniqueId = Guid.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }
        #endregion

        #region Extension methods

        #region Graph Permissions

        public async Task<IGraphPermissionCollection> GetShareLinksAsync()
        {
            await EnsurePropertiesAsync(y => y.SiteId, y => y.VroomItemID, y => y.VroomDriveID).ConfigureAwait(false);
            var apiCall = new ApiCall($"sites/{SiteId}/drives/{VroomDriveID}/items/{VroomItemID}/permissions?$filter=Link ne null", ApiType.GraphBeta);
            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (string.IsNullOrEmpty(response.Json))
            {
                throw new Exception("No values found");
            }

            var graphPermissions = SharingManager.DeserializeGraphPermissionsResponse(response.Json, PnPContext, this);

            return graphPermissions;
        }


        public IGraphPermissionCollection GetShareLinks()
        {
            return GetShareLinksAsync().GetAwaiter().GetResult();
        }

        public async Task DeleteShareLinksAsync()
        {
            var shareLinks = await GetShareLinksAsync().ConfigureAwait(false);
            foreach (var shareLink in shareLinks)
            {
                await shareLink.DeletePermissionAsync().ConfigureAwait(false);
            }
        }

        public void DeleteShareLinks()
        {
            DeleteShareLinksAsync().GetAwaiter().GetResult();
        }

        public async Task<IGraphPermission> CreateOrganizationalSharingLinkAsync(OrganizationalLinkOptions organizationalLinkOptions)
        {
            if (organizationalLinkOptions.Type == ShareType.CreateOnly)
            {
                throw new ArgumentException("An organizational link of type 'CreateOnly' can only be created on Folder level");
            }

            dynamic body = new ExpandoObject();
            body.scope = ShareScope.Organization;
            body.type = organizationalLinkOptions.Type;

            return await CreateSharingLinkAsync(body).ConfigureAwait(false);
            
        }

        public IGraphPermission CreateOrganizationalSharingLink(OrganizationalLinkOptions organizationalLinkOptions)
        {
            return CreateOrganizationalSharingLinkAsync(organizationalLinkOptions).GetAwaiter().GetResult();
        }

        public async Task<IGraphPermission> CreateAnonymousSharingLinkAsync(AnonymousLinkOptions anonymousLinkOptions)
        {
            if (anonymousLinkOptions.Type == ShareType.CreateOnly)
            {
                throw new ArgumentException("An anonymous link of type 'CreateOnly' can only be created on Folder level");
            }

            dynamic body = new ExpandoObject();

            body.scope = ShareScope.Anonymous;
            body.type = anonymousLinkOptions.Type;
            body.password = anonymousLinkOptions.Password;

            if (anonymousLinkOptions.ExpirationDateTime != DateTime.MinValue)
            {
                body.expirationDateTime = anonymousLinkOptions.ExpirationDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            }

            return await CreateSharingLinkAsync(body).ConfigureAwait(false);
        }

        public IGraphPermission CreateAnonymousSharingLink(AnonymousLinkOptions anonymousLinkOptions)
        {
            return CreateAnonymousSharingLinkAsync(anonymousLinkOptions).GetAwaiter().GetResult();
        }

        private async Task<IGraphPermission> CreateSharingLinkAsync(dynamic body)
        {
            await EnsurePropertiesAsync(y => y.SiteId, y => y.VroomItemID, y => y.VroomDriveID).ConfigureAwait(false);

            var apiCall = new ApiCall($"sites/{SiteId}/drives/{VroomDriveID}/items/{VroomItemID}/createLink", ApiType.GraphBeta, jsonBody: JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_WriteIndentedFalse_CamelCase_JsonStringEnumConverter));
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            {
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json);
                return SharingManager.DeserializeGraphPermission(json, PnPContext, this);
            }
            else
            {
                throw new Exception("Error occured during creation");
            }
        }

        public IGraphPermission CreateAnonymousSharingLink(OrganizationalLinkOptions organizationalLinkOptions)
        {
            return CreateOrganizationalSharingLinkAsync(organizationalLinkOptions).GetAwaiter().GetResult();
        }

        public async Task<IGraphPermission> CreateUserSharingLinkAsync(UserLinkOptions userLinkOptions)
        {
            if (userLinkOptions.Type == ShareType.CreateOnly)
            {
                throw new ArgumentException("A user link of type 'CreateOnly' can only be created on Folder level");
            }

            if (userLinkOptions.Recipients == null || userLinkOptions.Recipients.Count == 0)
            {
                throw new ArgumentException("We need to have atleast one recipient with whom we want to share the link");
            }

            dynamic body = new ExpandoObject();

            body.scope = ShareScope.Users;
            body.type = userLinkOptions.Type;
            body.recipients = userLinkOptions.Recipients;

            return await CreateSharingLinkAsync(body).ConfigureAwait(false);
        }

        public IGraphPermission CreateUserSharingLink(UserLinkOptions userLinkOptions)
        {
            return CreateUserSharingLinkAsync(userLinkOptions).GetAwaiter().GetResult();
        }

        public async Task<IGraphPermission> CreateSharingInviteAsync(InviteOptions inviteOptions)
        {
            if (!inviteOptions.RequireSignIn && !inviteOptions.SendInvitation)
            {
                throw new ArgumentException("RequireSignIn and SendInvitation cannot both be false");
            }

            await EnsurePropertiesAsync(y => y.SiteId, y => y.VroomItemID, y => y.VroomDriveID).ConfigureAwait(false);
            dynamic body = new ExpandoObject();
            body.requireSignIn = inviteOptions.RequireSignIn;
            body.sendInvitation = inviteOptions.SendInvitation;
            body.roles = inviteOptions.Roles.Select(y => y.ToString()).ToList();
            body.recipients = inviteOptions.Recipients;
            body.message = inviteOptions.Message;

            if (inviteOptions.ExpirationDateTime != DateTime.MinValue)
            {
                body.expirationDateTime = inviteOptions.ExpirationDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            }

            var apiCall = new ApiCall($"sites/{SiteId}/drives/{VroomDriveID}/items/{VroomItemID}/invite", ApiType.GraphBeta, jsonBody: JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase));

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            {
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

                if (json.TryGetProperty("value", out JsonElement dataRows))
                {
                    if (dataRows.GetArrayLength() == 1)
                    {
                        return SharingManager.DeserializeGraphPermission(dataRows.EnumerateArray().FirstOrDefault(), PnPContext, this);
                    }
                    
                }
                throw new Exception("Issue on creation");
            }
            else
            {
                throw new Exception("Error occured during creation");
            }
        }

        public IGraphPermission CreateSharingInvite(InviteOptions inviteOptions)
        {
            return CreateSharingInviteAsync(inviteOptions).GetAwaiter().GetResult();
        }


        #endregion

        #region GetContent
        public async Task<Stream> GetContentAsync(bool streamContent = false)
        {

#if NET5_0_OR_GREATER
            string downloadUrl;
            if (System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier == "browser-wasm")
            {
                // for WASM we use the browser's network stack and need to comply to CORS policies
                // hence we're not using the download.aspx page approach here
                downloadUrl = $"{PnPContext.Uri}/_api/Web/getFileById('{UniqueId}')/$value";
            }
            else
            {
                downloadUrl = $"{PnPContext.Uri}/_layouts/15/download.aspx?UniqueId={UniqueId}";
            }
#else
            string downloadUrl = $"{PnPContext.Uri}/_layouts/15/download.aspx?UniqueId={UniqueId}";
#endif

            var apiCall = new ApiCall(downloadUrl, ApiType.SPORest)
            {
                Interactive = true,
                ExpectBinaryResponse = true,
                StreamResponse = streamContent
            };

            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
            return response.BinaryContent;
        }

        public Stream GetContent(bool streamContent = false)
        {
            return GetContentAsync(streamContent).GetAwaiter().GetResult();
        }


        public async Task<byte[]> GetContentBytesAsync()
        {
            using (var contentStream = await GetContentAsync().ConfigureAwait(false))
            {
                return ((MemoryStream)contentStream).ToArray();
            }
        }

        public byte[] GetContentBytes()
        {
            return GetContentBytesAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region Publish
        public async Task PublishAsync(string comment = null)
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);
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
            var entity = EntityManager.GetClassInfo(GetType(), this);
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
            var entity = EntityManager.GetClassInfo(GetType(), this);
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
            var entity = EntityManager.GetClassInfo(GetType(), this);
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
            var entity = EntityManager.GetClassInfo(GetType(), this);
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
            var entity = EntityManager.GetClassInfo(GetType(), this);
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
            var entity = EntityManager.GetClassInfo(GetType(), this);
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
            var entity = EntityManager.GetClassInfo(GetType(), this);
            string undoCheckoutEndpointUrl = $"{entity.SharePointUri}/undoCheckout";

            var apiCall = new ApiCall(undoCheckoutEndpointUrl, ApiType.SPORest);

            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }
        #endregion

        #region Checkin

        public async Task CheckinAsync(string comment = null, CheckinType checkinType = CheckinType.MinorCheckIn)
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);
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
            var entity = EntityManager.GetClassInfo(GetType(), this);
            string checkinEndpointUrl = $"{entity.SharePointUri}/checkin(comment='{comment ?? string.Empty}',checkintype={(int)checkinType})";

            var apiCall = new ApiCall(checkinEndpointUrl, ApiType.SPORest);

            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }
        #endregion

        #region Approve

        public async Task ApproveAsync(string comment = null)
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);
            string checkinEndpointUrl = $"{entity.SharePointUri}/approve(comment='{comment ?? string.Empty}')";

            var apiCall = new ApiCall(checkinEndpointUrl, ApiType.SPORest);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void Approve(string comment = null)
        {
            CheckinAsync(comment).GetAwaiter().GetResult();
        }

        public void ApproveBatch(string comment = null)
        {
            ApproveBatchAsync(comment).GetAwaiter().GetResult();
        }

        public void ApproveBatch(Batch batch, string comment = null)
        {
            ApproveBatchAsync(batch, comment).GetAwaiter().GetResult();
        }

        public async Task ApproveBatchAsync(string comment = null)
        {
            await ApproveBatchAsync(PnPContext.CurrentBatch, comment).ConfigureAwait(false);
        }

        public async Task ApproveBatchAsync(Batch batch, string comment = null)
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);
            string approveEndpointUrl = $"{entity.SharePointUri}/approve(comment='{comment ?? string.Empty}')";

            var apiCall = new ApiCall(approveEndpointUrl, ApiType.SPORest);

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
            var entity = EntityManager.GetClassInfo(GetType(), this);
            string recycleEndpointUrl = $"{entity.SharePointUri}/recycle";

            var apiCall = new ApiCall(recycleEndpointUrl, ApiType.SPORest)
            {
                RemoveFromModel = true
            };

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
        #endregion

        #region CopyTo
        private ApiCall GetCopyToCrossSiteApiCall(string destinationUrl, bool overwrite, MoveCopyOptions options)
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
                    options.RetainEditorAndModifiedOnMove,
                    options.ShouldBypassSharedLocks
                }
            }.AsExpando();
            string body = JsonSerializer.Serialize(parameters, typeof(ExpandoObject));
            string copyToEndpointUrl = $"_api/SP.MoveCopyUtil.CopyFileByPath(overwrite=@a1)?@a1={overwrite.ToString().ToLower()}";

            return new ApiCall(copyToEndpointUrl, ApiType.SPORest, body);
        }

        private ApiCall GetCopyToApiCall(string destinationUrl, bool overwrite, MoveCopyOptions options)
        {
            options ??= new MoveCopyOptions() { KeepBoth = !overwrite };
            return GetCopyToCrossSiteApiCall(destinationUrl, overwrite, options);
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
            var entityInfo = EntityManager.GetClassInfo(GetType(), this);
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
                    options.RetainEditorAndModifiedOnMove,
                    options.ShouldBypassSharedLocks
                }
            }.AsExpando();
            string body = JsonSerializer.Serialize(parameters, typeof(ExpandoObject));
            string moveToEndpointUrl = $"_api/SP.MoveCopyUtil.MoveFileByPath(overwrite=@a1)?@a1={overwrite.ToString().ToLower()}";

            return new ApiCall(moveToEndpointUrl, ApiType.SPORest, body);
        }

        private ApiCall GetMoveToApiCall(string destinationUrl, MoveOperations moveOperations, MoveCopyOptions options)
        {
            // If same site and using move options that are not available via the more universal move method
            if (UrlUtility.IsSameSite(PnPContext.Uri, destinationUrl) && 
                // These operations cannnot be performed using the cross site move API call, hence we're falling back to the API call that only 
                // works inside the same site
               (moveOperations.HasFlag(MoveOperations.AllowBrokenThickets) || moveOperations.HasFlag(MoveOperations.BypassApprovePermission)))
            {
                return GetMoveToSameSiteApiCall(destinationUrl, moveOperations);
            }
            else
            {
                bool overwrite = moveOperations.HasFlag(MoveOperations.Overwrite);
                bool retainEditorAndModifiedOnMove = moveOperations.HasFlag(MoveOperations.RetainEditorAndModifiedOnMove);
                bool shouldByPassSharedLocks = moveOperations.HasFlag(MoveOperations.BypassSharedLock);

                options ??= new MoveCopyOptions()
                {
                    KeepBoth = !overwrite,
                    RetainEditorAndModifiedOnMove = retainEditorAndModifiedOnMove,
                    ShouldBypassSharedLocks = shouldByPassSharedLocks
                };
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

        #region Syntex
        public async Task<ISyntexClassifyAndExtractResult> ClassifyAndExtractAsync()
        {
            ApiCall apiCall = CreateClassifyAndExtractApiCall();
            var apiResult = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            // Process response
            return ProcessClassifyAndExtractResponse(apiResult.Json);
        }

        public ISyntexClassifyAndExtractResult ClassifyAndExtract()
        {
            return ClassifyAndExtractAsync().GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<ISyntexClassifyAndExtractResult>> ClassifyAndExtractBatchAsync(Batch batch)
        {
            ApiCall apiCall = CreateClassifyAndExtractApiCall();
            apiCall.RawSingleResult = new SyntexClassifyAndExtractResult();
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                var result = ProcessClassifyAndExtractResponse(json);
                (apiCall.RawSingleResult as SyntexClassifyAndExtractResult).Created = result.Created;
                (apiCall.RawSingleResult as SyntexClassifyAndExtractResult).DeliverDate = result.DeliverDate;
                (apiCall.RawSingleResult as SyntexClassifyAndExtractResult).ErrorMessage = result.ErrorMessage;
                (apiCall.RawSingleResult as SyntexClassifyAndExtractResult).Id = result.Id;
                (apiCall.RawSingleResult as SyntexClassifyAndExtractResult).Status = result.Status;
                (apiCall.RawSingleResult as SyntexClassifyAndExtractResult).StatusCode = result.StatusCode;
                (apiCall.RawSingleResult as SyntexClassifyAndExtractResult).TargetServerRelativeUrl = result.TargetServerRelativeUrl;
                (apiCall.RawSingleResult as SyntexClassifyAndExtractResult).TargetSiteUrl = result.TargetSiteUrl;
                (apiCall.RawSingleResult as SyntexClassifyAndExtractResult).TargetWebServerRelativeUrl = result.TargetWebServerRelativeUrl;
                (apiCall.RawSingleResult as SyntexClassifyAndExtractResult).WorkItemType = result.WorkItemType;
            };

            var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);

            return new BatchSingleResult<ISyntexClassifyAndExtractResult>(batch, batchRequest.Id, apiCall.RawSingleResult as ISyntexClassifyAndExtractResult);
        }

        public IBatchSingleResult<ISyntexClassifyAndExtractResult> ClassifyAndExtractBatch(Batch batch)
        {
            return ClassifyAndExtractBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<ISyntexClassifyAndExtractResult>> ClassifyAndExtractBatchAsync()
        {
            return await ClassifyAndExtractBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public IBatchSingleResult<ISyntexClassifyAndExtractResult> ClassifyAndExtractBatch()
        {
            return ClassifyAndExtractBatchAsync().GetAwaiter().GetResult();
        }

        private static ISyntexClassifyAndExtractResult ProcessClassifyAndExtractResponse(string json)
        {
            var root = JsonSerializer.Deserialize<JsonElement>(json);
            return new SyntexClassifyAndExtractResult
            {
                Created = root.GetProperty("Created").GetDateTime(),
                DeliverDate = root.GetProperty("DeliverDate").GetDateTime(),
                ErrorMessage = root.GetProperty("ErrorMessage").GetString(),
                StatusCode = root.GetProperty("StatusCode").GetInt32(),
                Id = root.GetProperty("ID").GetGuid(),
                Status = root.GetProperty("Status").GetString(),
                WorkItemType = root.GetProperty("Type").GetGuid(),
                TargetServerRelativeUrl = root.GetProperty("TargetServerRelativeUrl").GetString(),
                TargetSiteUrl = root.GetProperty("TargetSiteUrl").GetString(),
                TargetWebServerRelativeUrl = root.GetProperty("TargetWebServerRelativeUrl").GetString()
            };
        }

        private ApiCall CreateClassifyAndExtractApiCall()
        {
            var classifyAndExtractFile = new
            {
                __metadata = new { type = "Microsoft.Office.Server.ContentCenter.SPMachineLearningWorkItemEntityData" },
                Type = "AE9D4F24-EE84-4C0C-A972-A74CFFE939A1",
                TargetSiteId = PnPContext.Site.Id,
                TargetWebId = PnPContext.Web.Id,
                TargetUniqueId = UniqueId
            }.AsExpando();

            string body = JsonSerializer.Serialize(classifyAndExtractFile, PnPConstants.JsonSerializer_IgnoreNullValues);

            var apiCall = new ApiCall("_api/machinelearning/workitems", ApiType.SPORest, body);
            return apiCall;
        }
        #endregion

        #region Thumbnails
        public async Task<List<IThumbnail>> GetThumbnailsAsync(ThumbnailOptions options = null)
        {
            await EnsurePropertiesAsync(y => y.VroomItemID, y => y.VroomDriveID).ConfigureAwait(false);

            return await UnfurlHandler.GetThumbnailsAsync(PnPContext, VroomDriveID, VroomItemID, options).ConfigureAwait(false);
        }

        public List<IThumbnail> GetThumbnails(ThumbnailOptions options = null)
        {
            return GetThumbnailsAsync(options).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<IThumbnail>> GetThumbnailsBatchAsync(ThumbnailOptions options = null)
        {
            return await GetThumbnailsBatchAsync(PnPContext.CurrentBatch, options).ConfigureAwait(false);
        }

        public IEnumerableBatchResult<IThumbnail> GetThumbnailsBatch(ThumbnailOptions options = null)
        {
            return GetThumbnailsBatchAsync(options).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<IThumbnail>> GetThumbnailsBatchAsync(Batch batch, ThumbnailOptions options = null)
        {
            await EnsurePropertiesAsync(y => y.VroomItemID, y => y.VroomDriveID).ConfigureAwait(false);

            return await UnfurlHandler.GetThumbnailsBatchAsync(batch, PnPContext, VroomDriveID, VroomItemID, options).ConfigureAwait(false);
        }

        public IEnumerableBatchResult<IThumbnail> GetThumbnailsBatch(Batch batch, ThumbnailOptions options = null)
        {
            return GetThumbnailsBatchAsync(batch, options).GetAwaiter().GetResult();
        }
        #endregion

        #region Convert

        private static readonly Dictionary<string, string[]> supportedSourceFormats = new Dictionary<string, string[]>
        {
            { "pdf",  new string[] { "doc", "docx", "epub", "eml", "htm", "html", "md", "msg", "odp", "ods", "odt", "pps", "ppsx", "ppt", "pptx", "rtf", "tif", "tiff", "xls", "xlsm", "xlsx" } },
            { "html", new string[] { "eml", "md", "msg" } },
            { "glb",  new string[] { "cool", "fbx", "obj", "ply", "stl", "3mf" } },
            { "jpg",  new string[] { "3g2", "3gp", "3gp2", "3gpp", "3mf", "ai", "arw", "asf", "avi", "bas", "bash", "bat", "bmp", "c", "cbl", "cmd", "cool", "cpp", "cr2", "crw", "cs", "css", "csv", "cur", "dcm", "dcm30", "dic", "dicm", "dicom", "dng", "doc", "docx", "dwg", "eml", "epi", "eps", "epsf", "epsi", "epub", "erf", "fbx", "fppx", "gif", "glb", "h", "hcp", "heic", "heif", "htm", "html", "ico", "icon", "java", "jfif", "jpeg", "jpg", "js", "json", "key", "log", "m2ts", "m4a", "m4v", "markdown", "md", "mef", "mov", "movie", "mp3", "mp4", "mp4v", "mrw", "msg", "mts", "nef", "nrw", "numbers", "obj", "odp", "odt", "ogg", "orf", "pages", "pano", "pdf", "pef", "php", "pict", "pl", "ply", "png", "pot", "potm", "potx", "pps", "ppsx", "ppsxm", "ppt", "pptm", "pptx", "ps", "ps1", "psb", "psd", "py", "raw", "rb", "rtf", "rw1", "rw2", "sh", "sketch", "sql", "sr2", "stl", "tif", "tiff", "ts", "txt", "vb", "webm", "wma", "wmv", "xaml", "xbm", "xcf", "xd", "xml", "xpm", "yaml", "yml" } },
        };
         

        public async Task<Stream> ConvertToAsync(ConvertToOptions options)
        {
            await EnsurePropertiesAsync(y => y.Name, y => y.VroomItemID, y => y.VroomDriveID).ConfigureAwait(false);

            // Check file extension before converting
            CheckExtension(options.Format);

            string jpgOptions = "";
            if (options.Format == ConvertToFormat.Jpg)
            {
                jpgOptions = $"&width={options.JpgFormatWidth}&height={options.JpgFormatHeight}";
            }

            var convertEndpointUrl = $"sites/{PnPContext.Uri.DnsSafeHost},{PnPContext.Site.Id},{PnPContext.Web.Id}/drives/{VroomDriveID}/items/{VroomItemID}/content?format={options.Format.ToString().ToLowerInvariant()}{jpgOptions}";

            ApiType typeToUse = ApiType.GraphBeta;
            if (options.Format == ConvertToFormat.Pdf)
            {
                typeToUse = ApiType.Graph;
            }

            var apiCall = new ApiCall(convertEndpointUrl, typeToUse)
            {
                Interactive = true,
                StreamResponse = options.StreamContent,
                ExpectBinaryResponse = true,
                Headers = new Dictionary<string, string>()
                {
                    { "Accept", "*/*" }
                }
            };

            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
            
            return response.BinaryContent;
        }

        public Stream ConvertTo(ConvertToOptions options)
        {
            return ConvertToAsync(options).GetAwaiter().GetResult();
        }

        private void CheckExtension(ConvertToFormat format)
        {
            var sourceExtension = Name.Split('.').Last().ToLower();
            var targetExtension = format.ToString().ToLowerInvariant();

            if (!supportedSourceFormats[targetExtension].Contains(sourceExtension))
            {
                throw new ClientException(ErrorType.Unsupported, string.Format(PnPCoreResources.Exception_Unsupported_Extension_Converting_File, sourceExtension, targetExtension, supportedSourceFormats[targetExtension].ToList()));
            }
        }

        #endregion

        #region Get Analytics
        public async Task<List<IActivityStat>> GetAnalyticsAsync(AnalyticsOptions options = null)
        {
            return await ActivityHandler.GetAnalyticsAsync(this, options).ConfigureAwait(false);
        }

        public List<IActivityStat> GetAnalytics(AnalyticsOptions options = null)
        {
            return GetAnalyticsAsync(options).GetAwaiter().GetResult();
        }
        #endregion

        #region Preview

        public async Task<IFilePreview> GetPreviewAsync(PreviewOptions options = null)
        {
            await EnsurePropertiesAsync(y => y.VroomItemID, y => y.VroomDriveID).ConfigureAwait(false);

            if (options == null)
            {
                options = new PreviewOptions();
            }

            dynamic body = new ExpandoObject();

            if (options.Page != string.Empty)
            {
                body.page = options.Page;
            }

            if (options.Zoom != 0)
            {
                body.zoom = options.Zoom;
            }

            var apiCall = new ApiCall($"sites/{PnPContext.Uri.DnsSafeHost},{PnPContext.Site.Id},{PnPContext.Web.Id}/drives/{VroomDriveID}/items/{VroomItemID}/preview", ApiType.Graph, jsonBody: JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase));

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new MicrosoftGraphServiceException(ErrorType.GraphServiceError, (int)response.StatusCode, response.Json);
            }

            return GetPreviewFromResponse(response.Json);
        }

        public IFilePreview GetPreview(PreviewOptions options = null)
        {
            return GetPreviewAsync(options).GetAwaiter().GetResult();
        }

        private static IFilePreview GetPreviewFromResponse(string json)
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            var filePreview = new FilePreview();

            if (jsonElement.TryGetProperty("getUrl", out JsonElement getUrl))
            {
                filePreview.GetUrl = getUrl.GetString();
            }

            if (jsonElement.TryGetProperty("postUrl", out JsonElement postUrl))
            {
                filePreview.PostUrl = postUrl.GetString();
            }

            if (jsonElement.TryGetProperty("postParameters", out JsonElement postParameters))
            {
                filePreview.PostParameters = postParameters.GetString();
            }

            return filePreview;
        }

        #endregion

        #region Rename File
        public async Task RenameAsync(string name)
        {
            await EnsurePropertiesAsync(y => y.VroomItemID, y => y.VroomDriveID).ConfigureAwait(false);

            dynamic body = new
            {
                name
            };

            var apiCall = new ApiCall($"sites/{PnPContext.Uri.DnsSafeHost},{PnPContext.Site.Id},{PnPContext.Web.Id}/drives/{VroomDriveID}/items/{VroomItemID}", ApiType.Graph, JsonSerializer.Serialize(body, PnPConstants.JsonSerializer_IgnoreNullValues));
            await RequestAsync(apiCall, new HttpMethod("PATCH")).ConfigureAwait(false);
            // Update the Name property without marking the file as changed
            SetSystemValue(name, nameof(Name));
        }

        public void Rename(string name)
        {
            RenameAsync(name).GetAwaiter().GetResult();
        }
        #endregion

        #endregion

        #region Helper methods
        internal static bool ErrorIndicatesFileDoesNotExists(SharePointRestError error)
        {
            // Indicates the file did not exist
            if (error.HttpResponseCode == 404 && error.ServerErrorCode == -2130575338)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
