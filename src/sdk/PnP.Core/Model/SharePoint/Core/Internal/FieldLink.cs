using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Requests.ContentTypes;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// FieldLink class, write your custom code here
    /// </summary>
    [SharePointType("SP.FieldLink", Target = typeof(ContentType), Uri = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks')", Get = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks", LinqGet = "_api/Web/ContentTypes('{Parent.Id}')/FieldLinks")]
    internal sealed class FieldLink : BaseDataModel<IFieldLink>, IFieldLink
    {
        #region Construction
        public FieldLink()
        {

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            GetApiCallOverrideHandler = async (ApiCallRequest api) =>
            {
                if (IsViaContentTypeHub())
                {
                    var arguments = new Uri(api.ApiCall.Request).Query;
                    arguments ??= "";

                    api.ApiCall = new ApiCall($"{PnPContext.Uri.Scheme}://{PnPContext.Uri.DnsSafeHost}{PnPConstants.ContentTypeHubUrl}/_api/Web/ContentTypes('{(Parent as IContentType).StringId}')/FieldLinks{arguments}", api.ApiCall.Type, api.ApiCall.JsonBody, api.ApiCall.ReceivingProperty);
                }
                else
                {
                    if (Parent != null && Parent.Parent != null && Parent.Parent.Parent != null)
                    {
                        var parentType = Parent.Parent.Parent.GetType();

                        if (parentType == typeof(List))
                        {
                            var arguments = new Uri(api.ApiCall.Request).Query;
                            arguments ??= "";

                            api.ApiCall = new ApiCall($"{PnPContext.Uri.AbsoluteUri.TrimEnd('/')}/_api/Web/Lists(guid'{(Parent.Parent.Parent as IList).Id}')/ContentTypes('{(Parent as IContentType).StringId}')/FieldLinks{arguments}", api.ApiCall.Type, api.ApiCall.JsonBody, api.ApiCall.ReceivingProperty);
                        }
                    }
                }
                return api;
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously           
        }
        #endregion

        #region Properties
        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string FieldInternalName { get => GetValue<string>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public bool ReadOnly { get => GetValue<bool>(); set => SetValue(value); }

        public bool Required { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShowInDisplayForm { get => GetValue<bool>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }
        #endregion

        #region Method overrides
        internal override async Task BaseUpdate(Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            (Guid siteId, Guid webId) = await GetSiteAndWebIdsAsync().ConfigureAwait(false);

            var api = BuildUpdateApiCall(siteId, webId);

            await RawRequestAsync(api, HttpMethod.Post, "Update").ConfigureAwait(false);
        }

        internal override async Task BaseBatchUpdateAsync(Batch batch, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            (Guid siteId, Guid webId) = await GetSiteAndWebIdsAsync().ConfigureAwait(false);

            var api = BuildUpdateApiCall(siteId, webId);

            // Add the request to the batch
            await RawRequestBatchAsync(batch, api, HttpMethod.Post, "UpdateBatch").ConfigureAwait(false);
        }

        internal override async Task BaseDelete(Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            (Guid siteId, Guid webId) = await GetSiteAndWebIdsAsync().ConfigureAwait(false);

            var api = BuildDeleteApiCall(siteId, webId);

            await RawRequestAsync(api, HttpMethod.Post, "Delete").ConfigureAwait(false);
        }

        internal override async Task BaseDeleteBatchAsync(Batch batch, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            (Guid siteId, Guid webId) = await GetSiteAndWebIdsAsync().ConfigureAwait(false);

            var api = BuildDeleteApiCall(siteId, webId);

            // Add the request to the batch
            await RawRequestBatchAsync(batch, api, HttpMethod.Post, "DeleteBatch").ConfigureAwait(false);
        }

        private ApiCall BuildUpdateApiCall(Guid siteId, Guid webId)
        {
            List<IRequest<object>> csomRequests = new List<IRequest<object>>
            {
                new UpdateFieldLinkRequest(Parent.Parent as IContentType, this, siteId, webId, true)
            };

            return new ApiCall(csomRequests);
        }

        private ApiCall BuildDeleteApiCall(Guid siteId, Guid webId)
        {
            List<IRequest<object>> csomRequests = new List<IRequest<object>>
            {
                new DeleteFieldLinkRequest(Parent.Parent as IContentType, this, siteId, webId)
            };

            return new ApiCall(csomRequests);
        }
        #endregion

        #region Helper methods
        private bool IsViaContentTypeHub()
        {
            //return EntityManager.GetClassInfo(GetType(), this.Parent.Parent).SharePointTarget == typeof(ContentTypeHub);
            if (Parent != null && Parent.Parent != null && Parent.Parent.Parent != null)
            {
                if (Parent.Parent.Parent.GetType() == typeof(ContentTypeHub))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<(Guid, Guid)> GetSiteAndWebIdsAsync()
        {
            Guid siteId = Guid.Empty;
            Guid webId = Guid.Empty;
            if (Parent != null && Parent.Parent != null && Parent.Parent.Parent != null && Parent.Parent.Parent.Parent is ContentTypeHub hub)
            {
                hub.SiteId ??= await hub.GetSiteIdAsync().ConfigureAwait(false);

                if (!string.IsNullOrEmpty(hub.SiteId))
                {
                    var split = hub.SiteId.Split(',');
                    if (split.Length == 3)
                    {
                        siteId = Guid.Parse(split[1]);
                        webId = Guid.Parse(split[2]);
                    }
                }
                else
                {
                    throw new ClientException("No valid site id found");
                }
            }
            else
            {
                siteId = PnPContext.Site.Id;
                webId = PnPContext.Web.Id;
            }

            return (siteId, webId);
        }

        #endregion
    }
}
