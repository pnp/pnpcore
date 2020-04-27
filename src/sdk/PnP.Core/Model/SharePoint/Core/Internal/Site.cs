using Microsoft.Extensions.Logging;
using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Site class, write your custom code here
    /// </summary>
    [SharePointType("SP.Site", SharePointUri = "_api/Site")]
    [GraphType(GraphGet = "sites/{hostname}:{serverrelativepath}")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class Site
    {
        public Site()
        {
            MappingHandler = (FromJson input) =>
            {
                // Handle the mapping from json to the domain model for the cases which are not generically handled
                switch (input.TargetType.Name)
                {
                    case "SearchBoxInNavBar": return JsonMappingHelper.ToEnum<SearchBoxInNavBar>(input.JsonElement);
                }

                input.Log.LogWarning($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };
        }
    }
}
