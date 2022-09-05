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
                if (Parent != null && Parent.Parent != null && Parent.Parent.Parent != null)
                {
                    var parentType = Parent.Parent.Parent.GetType();

                    if (parentType == typeof(List))
                    {
                        var arguments = new Uri(api.ApiCall.Request).Query;
                        if (arguments == null)
                        {
                            arguments = "";
                        }

                        api.ApiCall = new ApiCall($"{PnPContext.Uri.AbsoluteUri}/_api/Web/Lists(guid'{(Parent.Parent.Parent as IList).Id}')/ContentTypes('{(Parent as IContentType).StringId}')/FieldLinks{arguments}", api.ApiCall.Type, api.ApiCall.JsonBody, api.ApiCall.ReceivingProperty);
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
            var api = BuildUpdateApiCall();

            await RawRequestAsync(api, HttpMethod.Post, "Update").ConfigureAwait(false);
        }

        internal override async Task BaseBatchUpdateAsync(Batch batch, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            var api = BuildUpdateApiCall();

            // Add the request to the batch
            await RawRequestBatchAsync(batch, api, HttpMethod.Post, "UpdateBatch").ConfigureAwait(false);
        }

        internal override async Task BaseDelete(Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            var api = BuildDeleteApiCall();

            await RawRequestAsync(api, HttpMethod.Post, "Delete").ConfigureAwait(false);
        }

        internal override async Task BaseDeleteBatchAsync(Batch batch, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            var api = BuildDeleteApiCall();

            // Add the request to the batch
            await RawRequestBatchAsync(batch, api, HttpMethod.Post, "DeleteBatch").ConfigureAwait(false);
        }

        private ApiCall BuildUpdateApiCall()
        {
            List<IRequest<object>> csomRequests = new List<IRequest<object>>
            {
                new UpdateFieldLinkRequest(Parent.Parent as IContentType, this, true)
            };

            return new ApiCall(csomRequests);
        }

        private ApiCall BuildDeleteApiCall()
        {
            List<IRequest<object>> csomRequests = new List<IRequest<object>>
            {
                new DeleteFieldLinkRequest(Parent.Parent as IContentType, this)
            };

            return new ApiCall(csomRequests);
        }
        #endregion
    }
}
