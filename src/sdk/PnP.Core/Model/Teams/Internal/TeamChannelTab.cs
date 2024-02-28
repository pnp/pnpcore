using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Text.Json;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = tabUri)]
    internal sealed class TeamChannelTab : BaseDataModel<ITeamChannelTab>, ITeamChannelTab
    {
        private const string baseUri = "teams/{Site.GroupId}/channels/{Parent.GraphId}/tabs";
        private const string tabUri = baseUri + "/{GraphId}";

        #region Construction
        public TeamChannelTab()
        {
            // Handler to construct the Add request for this channel
            AddApiCallHandler = async (keyValuePairs) =>
            {
                // Define the JSON body of the update request based on the actual changes
                dynamic tab = new ExpandoObject();
                tab.displayName = DisplayName;

                string teamsAppId = keyValuePairs[TeamChannelTabConstants.TeamsAppId].ToString();
                tab.teamsAppId = teamsAppId;

                switch (teamsAppId)
                {
                    case TeamChannelTabConstants.WikiAppId:
                    case TeamChannelTabConstants.SharePointPageOrListAppId:
                    case TeamChannelTabConstants.PowerBiAppId:
                    case TeamChannelTabConstants.FormsAppId:
                    case TeamChannelTabConstants.OneNoteAppId:
                    case TeamChannelTabConstants.StreamsAppId:
                    case TeamChannelTabConstants.PlannerAppId:
                        break;
                    default:
                        {
                            tab.Configuration = new ExpandoObject();

                            if (Configuration.IsPropertyAvailable(p => p.EntityId))
                            {
                                tab.Configuration.EntityId = Configuration.EntityId;
                            }
                            if (Configuration.IsPropertyAvailable(p => p.ContentUrl))
                            {
                                tab.Configuration.ContentUrl = Configuration.ContentUrl;
                            }
                            if (Configuration.IsPropertyAvailable(p => p.RemoveUrl))
                            {
                                tab.Configuration.RemoveUrl = Configuration.RemoveUrl;
                            }
                            if (Configuration.IsPropertyAvailable(p => p.WebsiteUrl))
                            {
                                tab.Configuration.WebsiteUrl = Configuration.WebsiteUrl;
                            }
                            break;
                        }
                }

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(tab, typeof(ExpandoObject), PnPConstants.JsonSerializer_WriteIndentedFalse);

                var parsedApiCall = await ApiHelper.ParseApiRequestAsync(this, baseUri).ConfigureAwait(false);
                return new ApiCall(parsedApiCall, ApiType.GraphBeta, bodyContent);
            };
        }
        #endregion

        #region Properties
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public Uri WebUrl { get => GetValue<Uri>(); set => SetValue(value); }

        // Beta only property
        public string SortOrderIndex { get => GetValue<string>(); set => SetValue(value); }

        public ITeamChannelTabConfiguration Configuration { get => GetModelValue<ITeamChannelTabConfiguration>(); set => SetModelValue(value); }

        public ITeamApp TeamsApp { get => GetModelValue<ITeamApp>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion
    }
}
