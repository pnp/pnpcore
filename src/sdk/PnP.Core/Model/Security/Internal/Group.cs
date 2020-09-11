using Microsoft.Extensions.Logging;
using PnP.Core.Services;

namespace PnP.Core.Model.Security
{
    [GraphType(Get = "groups/{GraphId}")]
    internal partial class Group
    {
        internal Group()
        {
            MappingHandler = (FromJson input) =>
            {
                switch (input.TargetType.Name)
                {
                    case "GroupVisibility": return JsonMappingHelper.ToEnum<GroupVisibility>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };
        }
    }
}
