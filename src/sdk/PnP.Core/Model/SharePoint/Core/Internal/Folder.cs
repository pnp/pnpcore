using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Folder class, write your custom code here
    /// </summary>
    [SharePointType("SP.Folder", Target = typeof(Web), Uri = "_api/Web/getFolderById('{Id}')", LinqGet = "_api/Web/Folders")]
    [SharePointType("SP.Folder", Target = typeof(Folder), Uri = "_api/Web/getFolderById('{Id}')", Get = "_api/Web/getFolderById('{Parent.Id}')/Folders", LinqGet = "_api/Web/getFolderById('{Parent.Id}')/Folders")]
    [SharePointType("SP.Folder", Target = typeof(List), Uri = "_api/Web/Lists(guid'{Parent.Id}')/rootFolder", LinqGet = "_api/Web/Lists(guid'{Parent.Id}')/Folders")]
    internal partial class Folder : BaseDataModel<IFolder>, IFolder
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

                return new ApiCall($"{entity.SharePointGet}/Add('{Name}')", ApiType.SPORest);
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

        public IFileCollection Files { get => GetModelCollectionValue<IFileCollection>(); }

        public IListItem ListItemAllFields { get => GetModelValue<IListItem>(); }

        public IFolder ParentFolder { get => GetModelValue<IFolder>(); }

        public IPropertyValues Properties { get => GetModelValue<IPropertyValues>(); }

        public IStorageMetrics StorageMetrics { get => GetModelValue<IStorageMetrics>(); }

        public IFolderCollection Folders { get => GetModelCollectionValue<IFolderCollection>(); }

        [KeyProperty(nameof(UniqueId))]
        public override object Key { get => UniqueId; set => UniqueId = Guid.Parse(value.ToString()); }
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
        #endregion
    }
}
