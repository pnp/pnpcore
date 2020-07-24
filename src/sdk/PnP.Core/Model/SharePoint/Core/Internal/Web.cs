using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Web;

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
                    case "SearchScopes": return JsonMappingHelper.ToEnum<SearchScopes>(input.JsonElement);
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
        public async Task<IFolder> GetFolderByServerRelativeUrlAsync(string serverRelativeUrl)
        {
            // Instantiate a folder, link it the Web as parent and provide it a context. This folder will not be included in the current model
            Folder folder = new Folder()
            {
                PnPContext = this.PnPContext,
                Parent = this
            };

            ApiCall apiCall = BuildGetFolderByRelativeUrlApiCall(serverRelativeUrl);

            await folder.RequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            return folder;
        }

        public IFolder GetFolderByServerRelativeUrl(string serverRelativeUrl)
        {
            return GetFolderByServerRelativeUrlAsync(serverRelativeUrl).GetAwaiter().GetResult();
        }

        public async Task<IFolder> GetFolderByServerRelativeUrlBatchAsync(Batch batch, string serverRelativeUrl)
        {
            // Instantiate a folder, link it the Web as parent and provide it a context. This folder will not be included in the current model
            Folder folder = new Folder()
            {
                PnPContext = this.PnPContext,
                Parent = this
            };

            ApiCall apiCall = BuildGetFolderByRelativeUrlApiCall(serverRelativeUrl);

            await folder.RequestBatchAsync(batch, apiCall, HttpMethod.Get).ConfigureAwait(false);

            return folder;
        }

        public IFolder GetFolderByServerRelativeUrlBatch(Batch batch, string serverRelativeUrl)
        {
            return GetFolderByServerRelativeUrlBatchAsync(batch, serverRelativeUrl).GetAwaiter().GetResult();
        }

        public async Task<IFolder> GetFolderByServerRelativeUrlBatchAsync(string serverRelativeUrl)
        {
            return await GetFolderByServerRelativeUrlBatchAsync(PnPContext.CurrentBatch, serverRelativeUrl).ConfigureAwait(false);
        }

        public IFolder GetFolderByServerRelativeUrlBatch(string serverRelativeUrl)
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

        #endregion
    }
}
