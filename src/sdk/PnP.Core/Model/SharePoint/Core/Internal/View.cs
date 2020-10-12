using Microsoft.Extensions.Logging;
using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// View class, write your custom code here
    /// </summary>
    [SharePointType("SP.View", Uri = "_api/web/lists/getbyid(guid'{Parent.Id}')/Views({Id})", Get = "_api/web/lists(guid'{Parent.Id}')/Views", LinqGet = "_api/web/lists(guid'{Parent.Id}')/Views")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class View
    {
        public View()
        {
            // Starting Point: https://s-kainet.github.io/sp-rest-explorer/#/entity/SP.View


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
        }
    }
}
