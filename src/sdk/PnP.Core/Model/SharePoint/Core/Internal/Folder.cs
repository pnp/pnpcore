using PnP.Core.Model.Security;
using PnP.Core.Services;
using PnP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Folder class, write your custom code here
    /// </summary>
    [SharePointType("SP.Folder", Target = typeof(Web), Uri = "_api/Web/getFolderById('{Id}')", LinqGet = "_api/Web/Folders")]
    [SharePointType("SP.Folder", Target = typeof(Folder), Uri = "_api/Web/getFolderById('{Id}')", Get = "_api/Web/getFolderById('{Parent.Id}')/Folders", LinqGet = "_api/Web/getFolderById('{Parent.Id}')/Folders")]
    [SharePointType("SP.Folder", Target = typeof(List), Uri = "_api/Web/Lists(guid'{Parent.Id}')/rootFolder", LinqGet = "_api/Web/Lists(guid'{Parent.Id}')/Folders")]
    [SharePointType("SP.Folder", Target = typeof(ListItem), Uri = "_api/Web/Lists(guid'{List.Id}')/Items({Parent.Id})/Folder")]
    internal sealed class Folder : BaseDataModel<IFolder>, IFolder
    {
        #region Construction
        public Folder()
        {
            // Handler to construct the Add request for this folder
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (keyValuePairs) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                // Given this method can apply on both Web.ContentTypes as List.ContentTypes we're getting the entity info which will 
                // automatically provide the correct 'parent'
                var entity = EntityManager.GetClassInfo(GetType(), this);

                //// Adding new content types on a list is not something we should allow
                //if (entity.Target == typeof(List))
                //{
                //throw new ClientException(ErrorType.Unsupported, OnPCoreResources.Exception_Unsupported_AddingContentTypeToList);
                //}

                string encodedPath = WebUtility.UrlEncode(Name.Replace("'", "''")).Replace("+", "%20");
                return new ApiCall($"{entity.SharePointGet}/AddUsingPath(decodedurl='{encodedPath}')", ApiType.SPORest);
            };
        }
        #endregion

        #region Properties
        public bool Exists { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsWOPIEnabled { get => GetValue<bool>(); set => SetValue(value); }

        // NOTE: Is implemented only using SPO Rest for now
        //[GraphProperty("folder.childCount")]
        public int ItemCount { get => GetValue<int>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string ProgID { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public DateTime TimeCreated { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime TimeLastModified { get => GetValue<DateTime>(); set => SetValue(value); }

        public Guid UniqueId { get => GetValue<Guid>(); set => SetValue(value); }

        public string WelcomePage { get => GetValue<string>(); set => SetValue(value); }

        internal string DriveItemId 
        { 
            get
            {
                if (PnPContext.Web.IsPropertyAvailable(p=>p.Id) && PnPContext.Site.IsPropertyAvailable(p=>p.Id) && !UniqueId.Equals(Guid.Empty))
                {
                    return DriveHelper.EncodeDriveItemId(PnPContext.Site.Id, PnPContext.Web.Id, UniqueId);
                }

                return null;
            }
        }

        public IContentTypeIdCollection ContentTypeOrder { get => GetModelCollectionValue<IContentTypeIdCollection>(); }

        public IContentTypeIdCollection UniqueContentTypeOrder { get => GetModelCollectionValue<IContentTypeIdCollection>(); }

        public IFileCollection Files { get => GetModelCollectionValue<IFileCollection>(); }

        public IListItem ListItemAllFields { get => GetModelValue<IListItem>(); }

        public IFolder ParentFolder { get => GetModelValue<IFolder>(); }

        public IPropertyValues Properties { get => GetModelValue<IPropertyValues>(); }

        public IStorageMetrics StorageMetrics { get => GetModelValue<IStorageMetrics>(); }

        public IFolderCollection Folders { get => GetModelCollectionValue<IFolderCollection>(); }

        [KeyProperty(nameof(UniqueId))]
        public override object Key { get => UniqueId; set => UniqueId = Guid.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }
        #endregion

        #region Extension methods

        #region Add sub folder
        /// <summary>
        /// Add a folder to the current folder.
        /// </summary>
        /// <param name="name">The name of the folder to add.</param>
        /// <returns>The added folder.</returns>
        public async Task<IFolder> AddFolderAsync(string name)
        {
            return await Folders.AddAsync(name).ConfigureAwait(false);
        }

        /// <summary>
        /// Add a folder to the current folder.
        /// </summary>
        /// <param name="name">The name of the folder to add.</param>
        /// <returns>The added folder.</returns>
        public IFolder AddFolder(string name)
        {
            return AddFolderAsync(name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Add a folder to the current folder via batch.
        /// </summary>
        /// <param name="name">The name of the folder to add.</param>
        /// <param name="batch">Batch to add the reques to</param>
        /// <returns>The added folder.</returns>
        public async Task<IFolder> AddFolderBatchAsync(Batch batch, string name)
        {
            return await Folders.AddBatchAsync(batch, name).ConfigureAwait(false);
        }

        /// <summary>
        /// Add a folder to the current folder via batch.
        /// </summary>
        /// <param name="name">The name of the folder to add.</param>
        /// <param name="batch">Batch to add the reques to</param>
        /// <returns>The added folder.</returns>
        public IFolder AddFolderBatch(Batch batch, string name)
        {
            return AddFolderBatchAsync(batch, name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Add a folder to the current folder via batch.
        /// </summary>
        /// <param name="name">The name of the folder to add.</param>
        /// <returns>The added folder.</returns>
        public async Task<IFolder> AddFolderBatchAsync(string name)
        {
            return await Folders.AddBatchAsync(PnPContext.CurrentBatch, name).ConfigureAwait(false);
        }

        /// <summary>
        /// Add a folder to the current folder via batch.
        /// </summary>
        /// <param name="name">The name of the folder to add.</param>
        /// <returns>The added folder.</returns>
        public IFolder AddFolderBatch(string name)
        {
            return AddFolderBatchAsync(name).GetAwaiter().GetResult();
        }
        #endregion

        #region Copy To
        private ApiCall GetCopyToApiCall(string destinationUrl, MoveCopyOptions options)
        {
            string destUrl = UrlUtility.EnsureAbsoluteUrl(PnPContext.Uri, destinationUrl).ToString();
            string srcUrl = UrlUtility.EnsureAbsoluteUrl(PnPContext.Uri, ServerRelativeUrl).ToString();

            if (options == null)
            {
                // If no options are specified, use the CopyTo API endpoint
                var parameters = new { destUrl, srcUrl }.AsExpando();
                string body = JsonSerializer.Serialize(parameters, typeof(ExpandoObject));
                return new ApiCall("_api/SP.MoveCopyUtil.CopyFolder()", ApiType.SPORest, body);
            }
            else
            {
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
                return new ApiCall("_api/SP.MoveCopyUtil.CopyFolderByPath()", ApiType.SPORest, body);
            }
        }

        public void CopyTo(string destinationUrl, MoveCopyOptions options = null)
        {
            CopyToAsync(destinationUrl, options).GetAwaiter().GetResult();
        }

        public async Task CopyToAsync(string destinationUrl, MoveCopyOptions options = null)
        {
            var apiCall = GetCopyToApiCall(destinationUrl, options);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void CopyToBatch(string destinationUrl, MoveCopyOptions options = null)
        {
            CopyToBatchAsync(destinationUrl, options).GetAwaiter().GetResult();
        }

        public void CopyToBatch(Batch batch, string destinationUrl, MoveCopyOptions options = null)
        {
            CopyToBatchAsync(batch, destinationUrl, options).GetAwaiter().GetResult();
        }

        public async Task CopyToBatchAsync(string destinationUrl, MoveCopyOptions options = null)
        {
            await CopyToBatchAsync(PnPContext.CurrentBatch, destinationUrl, options).ConfigureAwait(false);
        }

        public async Task CopyToBatchAsync(Batch batch, string destinationUrl, MoveCopyOptions options = null)
        {
            var apiCall = GetCopyToApiCall(destinationUrl, options);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }
        #endregion

        #region Move To
        private ApiCall GetMoveToApiCall(string destinationUrl, MoveCopyOptions options)
        {
            string destUrl = UrlUtility.EnsureAbsoluteUrl(PnPContext.Uri, destinationUrl).ToString();
            string srcUrl = UrlUtility.EnsureAbsoluteUrl(PnPContext.Uri, ServerRelativeUrl).ToString();

            if (options == null)
            {
                // If no options are specified, use the CopyTo API endpoint
                var parameters = new { destUrl, srcUrl }.AsExpando();
                string body = JsonSerializer.Serialize(parameters, typeof(ExpandoObject));
                return new ApiCall("_api/SP.MoveCopyUtil.MoveFolder()", ApiType.SPORest, body);
            }
            else
            {
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
                return new ApiCall("_api/SP.MoveCopyUtil.MoveFolderByPath()", ApiType.SPORest, body);
            }
        }

        public async Task MoveToAsync(string destinationUrl, MoveCopyOptions options = null)
        {
            var apiCall = GetMoveToApiCall(destinationUrl, options);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void MoveTo(string destinationUrl, MoveCopyOptions options = null)
        {
            MoveToAsync(destinationUrl, options).GetAwaiter().GetResult();
        }

        public async Task MoveToBatchAsync(Batch batch, string destinationUrl, MoveCopyOptions options = null)
        {
            var apiCall = GetMoveToApiCall(destinationUrl, options);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void MoveToBatch(Batch batch, string destinationUrl, MoveCopyOptions options = null)
        {
            MoveToBatchAsync(batch, destinationUrl, options).GetAwaiter().GetResult();
        }

        public async Task MoveToBatchAsync(string destinationUrl, MoveCopyOptions options = null)
        {
            await MoveToBatchAsync(PnPContext.CurrentBatch, destinationUrl, options).ConfigureAwait(false);
        }

        public void MoveToBatch(string destinationUrl, MoveCopyOptions options = null)
        {
            MoveToBatchAsync(destinationUrl, options).GetAwaiter().GetResult();
        }
        #endregion

        #region EnsureFolder
        public async Task<IFolder> EnsureFolderAsync(string folderRelativeUrl)
        {
            if (string.IsNullOrEmpty(folderRelativeUrl))
            {
                throw new ArgumentNullException(nameof(folderRelativeUrl));
            }

            var currentFolder = this as IFolder;

            await currentFolder.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

            var childFolderNames = folderRelativeUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            bool currentFolderWasCreated = false;
            string currentUrl = currentFolder.ServerRelativeUrl;

            foreach (var folderName in childFolderNames)
            {
                currentUrl = $"{currentUrl}/{folderName}";

                if (!currentFolderWasCreated)
                {
                    try
                    {
                        currentFolder = await currentFolder.PnPContext.Web.GetFolderByServerRelativeUrlAsync(currentUrl).ConfigureAwait(false);
                    }
                    catch (SharePointRestServiceException)
                    {
                        currentFolder = await currentFolder.AddFolderAsync(folderName).ConfigureAwait(false);
                        currentFolderWasCreated = true;
                    }
                }
                else
                {
                    currentFolder = await currentFolder.AddFolderAsync(folderName).ConfigureAwait(false);
                }
            }

            return currentFolder;
        }

        public IFolder EnsureFolder(string folderRelativeUrl)
        {
            return EnsureFolderAsync(folderRelativeUrl).GetAwaiter().GetResult();
        }
        #endregion

        #region Get Changes

        public async Task<IList<IChange>> GetChangesAsync(ChangeQueryOptions query)
        {
            var apiCall = ChangeCollectionHandler.GetApiCall(this, query);
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            return ChangeCollectionHandler.Deserialize(response, this, PnPContext).ToList();
        }

        public IList<IChange> GetChanges(ChangeQueryOptions query)
        {
            return GetChangesAsync(query).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<IChange>> GetChangesBatchAsync(Batch batch, ChangeQueryOptions query)
        {
            var apiCall = ChangeCollectionHandler.GetApiCall(this, query);
            apiCall.RawEnumerableResult = new List<IChange>();
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                var batchFirstRequest = batch.Requests.First().Value;
                ApiCallResponse response = new ApiCallResponse(apiCall, json, HttpStatusCode.OK, batchFirstRequest.Id, batchFirstRequest.ResponseHeaders);
                ((List<IChange>)apiCall.RawEnumerableResult).AddRange(ChangeCollectionHandler.Deserialize(response, this, PnPContext).ToList());
            };

            var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);

            return new BatchEnumerableBatchResult<IChange>(batch, batchRequest.Id, (IReadOnlyList<IChange>)apiCall.RawEnumerableResult);
        }

        public IEnumerableBatchResult<IChange> GetChangesBatch(Batch batch, ChangeQueryOptions query)
        {
            return GetChangesBatchAsync(batch, query).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<IChange>> GetChangesBatchAsync(ChangeQueryOptions query)
        {
            return await GetChangesBatchAsync(PnPContext.CurrentBatch, query).ConfigureAwait(false);
        }

        public IEnumerableBatchResult<IChange> GetChangesBatch(ChangeQueryOptions query)
        {
            return GetChangesBatchAsync(query).GetAwaiter().GetResult();
        }

        #endregion

        #region Syntex Support
        public async Task<ISyntexClassifyAndExtractResult> ClassifyAndExtractOffPeakAsync()
        {
            // Build the API call
            var apiCall = SyntexClassifyAndExtract.CreateClassifyAndExtractApiCall(PnPContext, UniqueId, true);
            // Send the call to server
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            // Parse the response json
            return SyntexClassifyAndExtract.ProcessClassifyAndExtractResponse(response.Json);
        }

        public ISyntexClassifyAndExtractResult ClassifyAndExtractOffPeak()
        {
            return ClassifyAndExtractOffPeakAsync().GetAwaiter().GetResult();
        }

        #endregion

        #region Files

        public async Task<List<IFile>> FindFilesAsync(string match)
        {
            match = WildcardToRegex(match);

            return await ParseFiles(this, match).ConfigureAwait(false);
        }

        public List<IFile> FindFiles(string match)
        {
            return FindFilesAsync(match).GetAwaiter().GetResult();
        }

        private async Task<List<IFile>> ParseFiles(IFolder folder, string match)
        {
            var foundFiles = new List<IFile>();
            IFileCollection files = folder.Files;

            foreach (File file in files)
            {
                if (Regex.IsMatch(file.Name, match, RegexOptions.IgnoreCase))
                {
                    foundFiles.Add(file);
                }
            }

            foreach (IFolder subfolder in folder.Folders)
            {
                foundFiles.AddRange(await ParseFiles(subfolder, match).ConfigureAwait(false));
            }

            return foundFiles;
        }

        private static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).
                               Replace(@"\*", ".*").
                               Replace(@"\?", ".") + "$";
        }

        #endregion

        #region Graph interop
        public async Task<(string driveId, string driveItemId)> GetGraphIdsAsync()
        {
            string driveId = null;
            string driveItemId = null;

            // DriveId
            if (PnPContext.Web.IsPropertyAvailable(p => p.Id) && PnPContext.Site.IsPropertyAvailable(p => p.Id))
            {
                // We need to the id of the list hosting this folder, which can be obtained using different strategies

                // Option A: try walking the parent tree to see if there's an IList
                Guid docLibId = GetListIdFromFolder(this);

                if (docLibId == Guid.Empty)
                {
                    // Option B: check if the ListItemAllFields property is loaded with the parent list property
                    if (IsPropertyAvailable(p=>p.ListItemAllFields) && ListItemAllFields.IsPropertyAvailable(p=>p.ParentList))
                    {
                        docLibId = ListItemAllFields.ParentList.Id;
                    }
                }

                if (docLibId == Guid.Empty)
                {
                    // Option C: get id from property bag
                    if (IsPropertyAvailable(p => p.Properties))
                    {
                        docLibId = Folder.DiscoverDocLibId(Properties);
                    }
                }

                if (docLibId == Guid.Empty)
                {
                    // Option D: load the needed information - requires server roundtrip
                    var tempFolder = await GetAsync(p => p.Properties).ConfigureAwait(false);

                    if (tempFolder.IsPropertyAvailable(p => p.Properties))
                    {
                        docLibId = Folder.DiscoverDocLibId(tempFolder.Properties);
                    }
                }

                if (docLibId != Guid.Empty)
                {
                    driveId = DriveHelper.EncodeDriveId(PnPContext.Site.Id, PnPContext.Web.Id, docLibId);
                }
            }

            // DriveItemId
            if (!string.IsNullOrEmpty(driveId) && PnPContext.Web.IsPropertyAvailable(p => p.Id) && PnPContext.Site.IsPropertyAvailable(p => p.Id) && !UniqueId.Equals(Guid.Empty))
            {
                driveItemId = DriveHelper.EncodeDriveItemId(PnPContext.Site.Id, PnPContext.Web.Id, UniqueId);
            }


            return (driveId, driveItemId);
        }

        private static Guid DiscoverDocLibId(IPropertyValues properties)
        {
            string propertyToParse = null;
            if (properties.Values.ContainsKey("vti_listid") && !string.IsNullOrEmpty(properties["vti_listid"].ToString()))
            {
                propertyToParse = properties["vti_listid"].ToString();
            }
            else if (properties.Values.ContainsKey("vti_listname") && !string.IsNullOrEmpty(properties["vti_listname"].ToString()))
            {
                propertyToParse = properties["vti_listname"].ToString();
            }

            if (!string.IsNullOrEmpty(propertyToParse))
            {
                if (Guid.TryParse(propertyToParse, out Guid listGuid))
                {
                    return listGuid;
                }
            }

            return Guid.Empty;
        }

        private Guid GetListIdFromFolder(IDataModelParent folder)
        {
            if (folder != null)
            {
                if (folder.Parent != null && folder.Parent is List)
                {
                    return (folder.Parent as List).Id;
                }
                else
                {
                    if (folder.Parent == null)
                    {
                        return Guid.Empty;
                    }
                    else
                    {
                        return GetListIdFromFolder(folder.Parent);
                    }
                }
            }

            return Guid.Empty;
        }

        #endregion

        #region Graph Permissions

        public async Task<IGraphPermissionCollection> GetShareLinksAsync()
        {
            var (driveId, driveItemId) = await GetGraphIdsAsync().ConfigureAwait(false);

            var apiCall = new ApiCall($"sites/{PnPContext.Site.Id}/drives/{driveId}/items/{driveItemId}/permissions?$filter=Link ne null", ApiType.GraphBeta);
            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (string.IsNullOrEmpty(response.Json))
            {
                throw new Exception("No values found");
            }

            var graphPermissions = DeserializeGraphPermissionsResponse(response.Json, PnPContext, this);

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
            if (organizationalLinkOptions.Type == ShareType.Review)
            {
                throw new ArgumentException("An organizational sharing link of type 'Review' can only be created on File level");
            }

            if (organizationalLinkOptions.Type == ShareType.BlocksDownload)
            {
                throw new ArgumentException("An organizational sharing link of type 'Review' can only be created on File level");
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
            if (anonymousLinkOptions.Type == ShareType.Review)
            {
                throw new ArgumentException("An aonymous sharing link of type 'Review' can only be created on File level");
            }

            if (anonymousLinkOptions.Type == ShareType.BlocksDownload)
            {
                throw new ArgumentException("An anonymous sharing link of type 'Review' can only be created on File level");
            }

            dynamic body = new ExpandoObject();

            body.scope = ShareScope.Anonymous;
            body.type = anonymousLinkOptions.Type;
            body.password = anonymousLinkOptions.Password;

            if (anonymousLinkOptions.ExpirationDateTime != DateTime.MinValue)
            {
                body.expirationDateTime = anonymousLinkOptions.ExpirationDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }

            return await CreateSharingLinkAsync(body).ConfigureAwait(false);
        }

        public IGraphPermission CreateAnonymousSharingLink(AnonymousLinkOptions anonymousLinkOptions)
        {
            return CreateAnonymousSharingLinkAsync(anonymousLinkOptions).GetAwaiter().GetResult();
        }

        private async Task<IGraphPermission> CreateSharingLinkAsync(dynamic body)
        {
            var (driveId, driveItemId) = await GetGraphIdsAsync().ConfigureAwait(false);

            var apiCall = new ApiCall($"sites/{PnPContext.Site.Id}/drives/{driveId}/items/{driveItemId}/createLink", ApiType.GraphBeta, jsonBody: JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_WriteIndentedFalse_CamelCase_JsonStringEnumConverter));
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            {
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json);
                return DeserializeGraphPermission(json, PnPContext, this);
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
            if (userLinkOptions.Type == ShareType.Review)
            {
                throw new ArgumentException("A user sharing link of type 'Review' can only be created on File level");
            }

            if (userLinkOptions.Type == ShareType.BlocksDownload)
            {
                throw new ArgumentException("A user sharing link of type 'Review' can only be created on File level");
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

            dynamic body = new ExpandoObject();
            body.requireSignIn = inviteOptions.RequireSignIn;
            body.sendInvitation = inviteOptions.SendInvitation;
            body.roles = inviteOptions.Roles.Select(y => y.ToString()).ToList();
            body.recipients = inviteOptions.Recipients;
            body.message = inviteOptions.Message;

            if (inviteOptions.ExpirationDateTime != DateTime.MinValue)
            {
                body.expirationDateTime = inviteOptions.ExpirationDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }

            var (driveId, driveItemId) = await GetGraphIdsAsync().ConfigureAwait(false);

            var apiCall = new ApiCall($"sites/{PnPContext.Site.Id}/drives/{driveId}/items/{driveItemId}/invite", ApiType.GraphBeta, jsonBody: JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase));

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            {
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

                if (json.TryGetProperty("value", out JsonElement dataRows))
                {
                    if (dataRows.GetArrayLength() == 1)
                    {
                        return DeserializeGraphPermission(dataRows.EnumerateArray().FirstOrDefault(), PnPContext, this);
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
        #region Graph Permissions Deserialization Helper
        private IGraphPermissionCollection DeserializeGraphPermissionsResponse(string responseJson, PnPContext context, Folder folder)
        {
            var graphPermissions = new GraphPermissionCollection(context, folder);

            var json = JsonSerializer.Deserialize<JsonElement>(responseJson);

            if (json.TryGetProperty("value", out JsonElement dataRows))
            {
                if (dataRows.GetArrayLength() == 0)
                {
                    return graphPermissions;
                }

                foreach (var row in dataRows.EnumerateArray())
                {
                    graphPermissions.Add(DeserializeGraphPermission(row, context, folder));
                }
            }
            return graphPermissions;
        }

        private IGraphPermission DeserializeGraphPermission(JsonElement row, PnPContext context, Folder folder)
        {
            var returnPermission = new GraphPermission(context, folder);

            if (row.TryGetProperty("id", out JsonElement id))
            {
                returnPermission.Id = id.GetString();
            }

            if (row.TryGetProperty("roles", out JsonElement roles))
            {
                returnPermission.Roles = JsonSerializer.Deserialize<List<PermissionRole>>(roles.GetRawText(), PnPConstants.JsonSerializer_IgnoreNullValues_StringEnumConvertor);
            }

            if (row.TryGetProperty("hasPassword", out JsonElement hasPassword))
            {
                returnPermission.HasPassword = hasPassword.GetBoolean();
            }

            if (row.TryGetProperty("sharedId", out JsonElement sharedId))
            {
                returnPermission.ShareId = sharedId.GetString();
            }

            if (row.TryGetProperty("expirationDateTime", out JsonElement expirationDateTime))
            {
                returnPermission.ExpirationDateTime = expirationDateTime.GetDateTime();
            }

            if (row.TryGetProperty("grantedToV2", out JsonElement grantedToV2))
            {
                returnPermission.GrantedToV2 = DeserializeGrantedToV2(grantedToV2);
            }

            if (row.TryGetProperty("invitation", out JsonElement invitation))
            {
                returnPermission.Invitation = DeserializeInvitation(invitation);
            }

            if (row.TryGetProperty("grantedToIdentitiesV2", out JsonElement grantedToIdentitiesV2))
            {
                var identitySets = new List<ISharePointIdentitySet>();

                foreach (var grantedToIdentitiesV2Object in grantedToIdentitiesV2.EnumerateArray())
                {
                    identitySets.Add(DeserializeGrantedToV2(grantedToIdentitiesV2Object));
                }
                returnPermission.GrantedToIdentitiesV2 = identitySets;
            }

            if (row.TryGetProperty("link", out JsonElement link))
            {
                returnPermission.Link = DeserializeSharingLink(link);
            }

            return returnPermission;
        }

        private static ISharingInvitation DeserializeInvitation(JsonElement invitation)
        {
            var sharingInvitation = new SharingInvitation();

            if (invitation.TryGetProperty("signInRequired", out JsonElement signInRequired))
            {
                sharingInvitation.SignInRequired = signInRequired.GetBoolean();
            }
            if (invitation.TryGetProperty("email", out JsonElement email))
            {
                sharingInvitation.Email = email.GetString();
            }
            if (invitation.TryGetProperty("invitedBy", out JsonElement invitedBy))
            {
                sharingInvitation.InvitedBy = DeserializeInvitedBy(invitedBy);
            }
            return sharingInvitation;
        }

        private static IIdentitySet DeserializeInvitedBy(JsonElement invitedBy)
        {
            var identitySet = new IdentitySet();

            if (invitedBy.TryGetProperty("user", out JsonElement user))
            {
                identitySet.User = DeserializeIdentity(user);
            }

            if (invitedBy.TryGetProperty("application", out JsonElement application))
            {
                identitySet.Application = DeserializeIdentity(application);
            }

            if (invitedBy.TryGetProperty("device", out JsonElement device))
            {
                identitySet.Device = DeserializeIdentity(device);
            }

            return identitySet;
        }

        private static ISharingLink DeserializeSharingLink(JsonElement link)
        {
            var sharingLink = new SharingLink();

            if (link.TryGetProperty("scope", out JsonElement scope) && Enum.TryParse(scope.GetString(), true, out ShareScope shareScope))
            {
                sharingLink.Scope = shareScope;
            }
            if (link.TryGetProperty("type", out JsonElement type) && Enum.TryParse(type.GetString(), true, out ShareType shareType))
            {
                sharingLink.Type = shareType;
            }
            if (link.TryGetProperty("webUrl", out JsonElement webUrl))
            {
                sharingLink.WebUrl = webUrl.GetString();
            }
            if (link.TryGetProperty("webHtml", out JsonElement webHtml))
            {
                sharingLink.WebHtml = webHtml.GetString();
            }
            if (link.TryGetProperty("preventsDownload", out JsonElement preventsDownload))
            {
                sharingLink.PreventsDownload = preventsDownload.GetBoolean();
            }
            return sharingLink;
        }

        private static ISharePointIdentitySet DeserializeGrantedToV2(JsonElement grantedToV2)
        {
            var grantedV2Identity = new SharePointIdentitySet();

            if (grantedToV2.TryGetProperty("user", out JsonElement user))
            {
                grantedV2Identity.User = DeserializeIdentity(user);
            }

            if (grantedToV2.TryGetProperty("application", out JsonElement application))
            {
                grantedV2Identity.Application = DeserializeIdentity(application);
            }

            if (grantedToV2.TryGetProperty("device", out JsonElement device))
            {
                grantedV2Identity.Device = DeserializeIdentity(device);
            }

            if (grantedToV2.TryGetProperty("group", out JsonElement group))
            {
                grantedV2Identity.Group = DeserializeIdentity(group);
            }

            if (grantedToV2.TryGetProperty("siteUser", out JsonElement siteUser))
            {
                grantedV2Identity.SiteUser = DeserializeSharePointIdentity(siteUser);
            }

            if (grantedToV2.TryGetProperty("siteGroup", out JsonElement siteGroup))
            {
                grantedV2Identity.SiteGroup = DeserializeSharePointIdentity(siteGroup);
            }

            return grantedV2Identity;
        }

        private static ISharePointIdentity DeserializeSharePointIdentity(JsonElement row)
        {
            var sharePointIdentity = new SharePointIdentity();

            if (row.TryGetProperty("id", out JsonElement id))
            {
                sharePointIdentity.Id = id.GetString();
            }
            if (row.TryGetProperty("displayName", out JsonElement displayName))
            {
                sharePointIdentity.DisplayName = displayName.GetString();
            }
            if (row.TryGetProperty("loginName", out JsonElement loginName))
            {
                sharePointIdentity.LoginName = loginName.GetString();
            }
            if (row.TryGetProperty("email", out JsonElement email))
            {
                sharePointIdentity.Email = email.GetString();
            }
            return sharePointIdentity;
        }

        private static IIdentity DeserializeIdentity(JsonElement row)
        {
            var identity = new Identity();
            if (row.TryGetProperty("id", out JsonElement id))
            {
                identity.Id = id.GetString();
            }
            if (row.TryGetProperty("displayName", out JsonElement displayName))
            {
                identity.DisplayName = displayName.GetString();
            }
            if (row.TryGetProperty("email", out JsonElement email))
            {
                identity.Email = email.GetString();
            }
            return identity;
        }

        #endregion

        #endregion
    }
}
