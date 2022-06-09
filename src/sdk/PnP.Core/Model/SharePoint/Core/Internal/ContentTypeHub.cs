using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Content Type Hub class, write your custom code here
    /// </summary>
    [SharePointType("SP.Web", Uri = "_api/Web")]
    internal sealed class ContentTypeHub : BaseDataModel<IContentTypeHub>, IContentTypeHub
    {
        #region Construction
        public ContentTypeHub()
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            GetApiCallOverrideHandler = async (ApiCallRequest api) =>
            {
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                var request = api.ApiCall.Request.Replace(PnPContext.Uri.AbsolutePath, PnPConstants.ContentTypeHubUrl);
                api.ApiCall = new ApiCall(request, api.ApiCall.Type, api.ApiCall.JsonBody, api.ApiCall.ReceivingProperty);

                return api;
            };
        }
        #endregion

        #region Properties

        [SharePointProperty("ContentTypes")]
        public IContentTypeCollection ContentTypes { get => GetModelCollectionValue<IContentTypeCollection>(); }

        #endregion

    }
}
