using System;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = "teams/{Site.GroupId}", LinqGet = "teams")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class Team
    {
        public Team()
        {
            GetApiCallOverrideHandler = (ApiCallRequest api) =>
            {
                if (!PnPContext.Site.IsPropertyAvailable(p => p.GroupId) || PnPContext.Site.GroupId == Guid.Empty)
                {
                    api.CancelRequest("There is no Office 365 group attached to the current site");
                }

                return api;
            };

        }
    }
}
