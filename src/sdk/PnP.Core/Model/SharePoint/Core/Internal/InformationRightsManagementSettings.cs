using PnP.Core.Services;
using System.Net.Http;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// InformationRightsManagementSettings class, write your custom code here
    /// </summary>
    [SharePointType("SP.InformationRightsManagementSettings", Target = typeof(InformationRightsManagementSettings), Uri = "_api/web/lists/getbyid(guid'{Parent.Id}')/InformationRightsManagementSettings")]
    internal partial class InformationRightsManagementSettings
    {
        public InformationRightsManagementSettings()
        {
            //MappingHandler = (FromJson input) =>
            //{
            //// implement custom mapping logic
            //switch (input.TargetType.Name)
            //{
            //    case "SearchScopes": return JsonMappingHelper.ToEnum<SearchScopes>(input.JsonElement);
            //    case "SearchBoxInNavBar": return JsonMappingHelper.ToEnum<SearchBoxInNavBar>(input.JsonElement);                    
            //}
            //
            //input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");
            //
            //return null;
            //};

            //UpdateApiCallOverrideHandler = async (ApiCallRequest input) =>
            //{
            //    await this.RequestAsync(input.ApiCall, HttpMethod.Post).ConfigureAwait(false);

            //    input.CancelRequest();
            //    return input;
            //};

        }
    }
}
