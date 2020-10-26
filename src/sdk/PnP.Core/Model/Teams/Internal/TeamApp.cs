using Microsoft.Extensions.Logging;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = "teams/{Parent.GraphId}/installedapps/{GraphId}")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class TeamApp
    {
        public TeamApp()
        {
        }
    }
}
