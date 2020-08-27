using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Web class, write your custom code here
    /// </summary>
    [SharePointType("SP.Web", Uri = "_api/web", LinqGet = "_api/site/rootWeb/webinfos")]
    [GraphType(Get = "sites/{hostname}:{serverrelativepath}")]
    internal partial class Web
    {
        public Web()
        {
            PostMappingHandler = (json) =>
            {
                // implement post mapping handler in case you want to do extra data loading/mapping work
            };

            MappingHandler = (FromJson input) =>
            {
                // implement custom mapping logic

                //// Sample of field override, done by setting the UseCustomMapping = true field attribute
                //if (input.FieldName == "NoCrawl")
                //{
                //    return true;
                //}

                switch (input.TargetType.Name)
                {
                    case "SearchScopes": return JsonMappingHelper.ToEnum<SearchScope>(input.JsonElement);
                    case "SearchBoxInNavBar": return JsonMappingHelper.ToEnum<SearchBoxInNavBar>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };

            //GetApiCallOverrideHandler = (ApiCallRequest api) =>
            //{
            //    return api;
            //};

        }

        #region Extension methods

        #region Folders
        public async Task<IFolder> GetFolderByServerRelativeUrlAsync(string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions)
        {
            // Instantiate a folder, link it the Web as parent and provide it a context. This folder will not be included in the current model
            Folder folder = new Folder()
            {
                PnPContext = this.PnPContext,
                Parent = this
            };

            await folder.BaseGet(apiOverride: BuildGetFolderByRelativeUrlApiCall(serverRelativeUrl), fromJsonCasting: folder.MappingHandler, postMappingJson: folder.PostMappingHandler, expressions: expressions).ConfigureAwait(false);

            return folder;
        }



        public IFolder GetFolderByServerRelativeUrl(string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions)
        {
            return GetFolderByServerRelativeUrlAsync(serverRelativeUrl, expressions).GetAwaiter().GetResult();
        }

        public async Task<IFolder> GetFolderByServerRelativeUrlBatchAsync(Batch batch, string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions)
        {
            // Instantiate a folder, link it the Web as parent and provide it a context. This folder will not be included in the current model
            Folder folder = new Folder()
            {
                PnPContext = this.PnPContext,
                Parent = this
            };

            await folder.BaseBatchGetAsync(batch, apiOverride: BuildGetFolderByRelativeUrlApiCall(serverRelativeUrl), fromJsonCasting: folder.MappingHandler, postMappingJson: folder.PostMappingHandler, expressions: expressions).ConfigureAwait(false);

            return folder;
        }

        public IFolder GetFolderByServerRelativeUrlBatch(Batch batch, string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions)
        {
            return GetFolderByServerRelativeUrlBatchAsync(batch, serverRelativeUrl).GetAwaiter().GetResult();
        }

        public async Task<IFolder> GetFolderByServerRelativeUrlBatchAsync(string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions)
        {
            return await GetFolderByServerRelativeUrlBatchAsync(PnPContext.CurrentBatch, serverRelativeUrl).ConfigureAwait(false);
        }

        public IFolder GetFolderByServerRelativeUrlBatch(string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions)
        {
            return GetFolderByServerRelativeUrlBatchAsync(serverRelativeUrl).GetAwaiter().GetResult();
        }

        private static ApiCall BuildGetFolderByRelativeUrlApiCall(string serverRelativeUrl)
        {
            // NOTE WebUtility encode spaces to "+" instead of %20
            string encodedServerRelativeUrl = WebUtility.UrlEncode(serverRelativeUrl).Replace("+", "%20");
            var apiCall = new ApiCall($"_api/Web/getFolderByServerRelativeUrl('{encodedServerRelativeUrl}')", ApiType.SPORest);
            return apiCall;
        }

        #endregion

        #region Files
        public IFile GetFileByServerRelativeUrl(string serverRelativeUrl)
        {
            return GetFileByServerRelativeUrlAsync(serverRelativeUrl).GetAwaiter().GetResult();
        }

        public async Task<IFile> GetFileByServerRelativeUrlAsync(string serverRelativeUrl)
        {
            // Instantiate a file, link it the Web as parent and provide it a context. This folder will not be included in the current model
            File file = new File()
            {
                PnPContext = this.PnPContext,
                Parent = this
            };

            ApiCall apiCall = BuildGetFileByRelativeUrlApiCall(serverRelativeUrl);

            await file.RequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            return file;
        }

        public IFile GetFileByServerRelativeUrlBatch(Batch batch, string serverRelativeUrl)
        {
            return GetFileByServerRelativeUrlBatchAsync(batch, serverRelativeUrl).GetAwaiter().GetResult();
        }

        public IFile GetFileByServerRelativeUrlBatch(string serverRelativeUrl)
        {
            return GetFileByServerRelativeUrlBatchAsync(serverRelativeUrl).GetAwaiter().GetResult();
        }

        public async Task<IFile> GetFileByServerRelativeUrlBatchAsync(Batch batch, string serverRelativeUrl)
        {
            // Instantiate a file, link it the Web as parent and provide it a context. This folder will not be included in the current model
            File file = new File()
            {
                PnPContext = this.PnPContext,
                Parent = this
            };

            ApiCall apiCall = BuildGetFileByRelativeUrlApiCall(serverRelativeUrl);

            await file.RequestBatchAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            return file;
        }

        public async Task<IFile> GetFileByServerRelativeUrlBatchAsync(string serverRelativeUrl)
        {
            return await GetFileByServerRelativeUrlBatchAsync(PnPContext.CurrentBatch, serverRelativeUrl).ConfigureAwait(false);
        }

        private static ApiCall BuildGetFileByRelativeUrlApiCall(string serverRelativeUrl)
        {
            // NOTE WebUtility encode spaces to "+" instead of %20
            string encodedServerRelativeUrl = WebUtility.UrlEncode(serverRelativeUrl).Replace("+", "%20");
            var apiCall = new ApiCall($"_api/Web/getFileByServerRelativeUrl('{encodedServerRelativeUrl}')", ApiType.SPORest);
            return apiCall;
        }
        #endregion

        #endregion
    }
}
