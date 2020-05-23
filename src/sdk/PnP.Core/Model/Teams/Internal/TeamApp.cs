using Microsoft.Extensions.Logging;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = "teams/{Parent.GraphId}/installedapps/{GraphId}")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class TeamApp
    {
        internal TeamApp()
        {
            MappingHandler = (FromJson input) =>
            {
                switch (input.TargetType.Name)
                {
                    case "TeamsAppDistributionMethod": return JsonMappingHelper.ToEnum<TeamsAppDistributionMethod>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };
        }
    }
}
