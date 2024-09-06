using PnP.Core.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
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
            // As there's only one ContentTypeHub per tenant give it a fixed id
            Id = Guid.Parse("{DCC44C1F-8D59-42FC-B6D8-774758DF329E}");

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            GetApiCallOverrideHandler = async (ApiCallRequest api) =>
            {
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                var request = SwitchToContentTypeHubUrl(PnPContext.Uri, api.ApiCall.Request);
                api.ApiCall = new ApiCall(request, api.ApiCall.Type, api.ApiCall.JsonBody, api.ApiCall.ReceivingProperty);

                return api;
            };
        }
        #endregion

        #region Properties

        public IFieldCollection Fields { get => GetModelCollectionValue<IFieldCollection>(); }

        public IContentTypeCollection ContentTypes { get => GetModelCollectionValue<IContentTypeCollection>(); }

        internal string SiteId { get; set; }

        internal Guid Id { get; set; }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

        #endregion

        #region Methods

        public async Task<string> GetSiteIdAsync()
        {
            if (SiteId != null)
            {
                return SiteId;
            }

            var apiCall = BuildSiteIdApiCall();

            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new MicrosoftGraphServiceException(PnPCoreResources.Exception_RetrievingContentTypeHubSiteId);
            }

            var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

            if (json.TryGetProperty("id", out JsonElement id))
            {
                SiteId = id.GetString();
            }

            return SiteId;
        }

        private ApiCall BuildSiteIdApiCall()
        {
            return new ApiCall($"sites/{PnPContext.Uri.Host}:/sites/contenttypehub?$select=id", ApiType.Graph);
        }

        public string GetSiteId()
        {
            return GetSiteIdAsync().GetAwaiter().GetResult();
        }

        internal static string SwitchToContentTypeHubUrl(Uri contextUri, string api)
        {
            if (contextUri.Segments.Length == 1)
            {
                // For when then context was created for the root site collection
                if (Uri.IsWellFormedUriString(api, UriKind.Absolute))
                {
                    return api.Replace($"{contextUri.Scheme}://{contextUri.DnsSafeHost}", $"{contextUri.Scheme}://{contextUri.DnsSafeHost}{PnPConstants.ContentTypeHubUrl}");
                }
                else
                {
                    return $"{PnPConstants.ContentTypeHubUrl}{api}";
                }
            }
            else
            {
                return api.Replace(contextUri.AbsolutePath, PnPConstants.ContentTypeHubUrl);
            }
        }

        #endregion
    }
}
