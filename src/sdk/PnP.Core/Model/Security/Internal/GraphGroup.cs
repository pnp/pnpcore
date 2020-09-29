using Microsoft.Extensions.Logging;
using PnP.Core.Services;

namespace PnP.Core.Model.Security
{
    [GraphType(Get = "groups/{GraphId}")]
    internal partial class GraphGroup
    {
        internal GraphGroup()
        {
            MappingHandler = (FromJson input) =>
            {
                switch (input.TargetType.Name)
                {
                    case "GroupVisibility": return JsonMappingHelper.ToEnum<GroupVisibility>(input.JsonElement);
                }

                input.Log.LogDebug(PnPCoreResources.Log_Debug_JsonCannotMapField, input.FieldName);

                return null;
            };
        }
    }
}
