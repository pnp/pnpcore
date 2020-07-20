using System.Dynamic;
using System.Text.Json;
using PnP.Core.Services;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = V)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class TeamChannelTab
    {
        private const string baseUri = "teams/{Site.GroupId}/channels/{Parent.GraphId}/tabs";
        private const string V = baseUri + "/{GraphId}";

        public TeamChannelTab()
        {
            // Handler to construct the Add request for this channel
            AddApiCallHandler = async (keyValuePairs) =>
            {
                // Define the JSON body of the update request based on the actual changes
                dynamic tab = new ExpandoObject();
                tab.displayName = DisplayName;

                string teamsAppId = keyValuePairs["teamsAppId"].ToString();
                tab.teamsAppId = teamsAppId;

                switch (teamsAppId)
                {
                    case "com.microsoft.teamspace.tab.wiki": // Wiki, no configuration possible
                        break;
                    default: 
                        {
                            tab.Configuration = new ExpandoObject();

                            if (Configuration.IsPropertyAvailable<ITeamChannelTabConfiguration>(p=>p.EntityId))
                            {
                                tab.Configuration.EntityId = Configuration.EntityId;
                            }
                            if (Configuration.IsPropertyAvailable<ITeamChannelTabConfiguration>(p => p.ContentUrl))
                            {
                                tab.Configuration.ContentUrl = Configuration.ContentUrl;
                            }
                            if (Configuration.IsPropertyAvailable<ITeamChannelTabConfiguration>(p => p.RemoveUrl))
                            {
                                tab.Configuration.RemoveUrl = Configuration.RemoveUrl;
                            }
                            if (Configuration.IsPropertyAvailable<ITeamChannelTabConfiguration>(p => p.WebsiteUrl))
                            {
                                tab.Configuration.WebsiteUrl = Configuration.WebsiteUrl;
                            }
                            break;
                        }
                }

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(tab, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = false });

                var parsedApiCall = await ApiHelper.ParseApiRequestAsync(this, baseUri).ConfigureAwait(false);
                return new ApiCall(parsedApiCall, ApiType.GraphBeta, bodyContent);
            };
        }
    }
}
