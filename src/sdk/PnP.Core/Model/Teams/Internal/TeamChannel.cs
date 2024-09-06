using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Utilities;
using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = channelUri, LinqGet = baseUri)]
    internal sealed class TeamChannel : BaseDataModel<ITeamChannel>, ITeamChannel
    {
        private const string baseUri = "teams/{Parent.GraphId}/channels";
        private const string channelUri = baseUri + "/{GraphId}";

        #region Construction
        public TeamChannel()
        {

            // Handler to construct the Add request for this channel
            AddApiCallHandler = async (keyValuePairs) =>
            {
                // Define the JSON body of the update request based on the actual changes
                dynamic body = new ExpandoObject();
                body.displayName = DisplayName;
                if (!string.IsNullOrEmpty(Description))
                {
                    body.description = Description;
                }
                body.MembershipType = MembershipType;

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_WriteIndentedFalse);

                var apiCall = await ApiHelper.ParseApiRequestAsync(this, baseUri).ConfigureAwait(false);

                return new ApiCall(apiCall, ApiType.Graph, bodyContent);
            };

            // Validation handler to prevent updating the general channel
            ValidateUpdateHandler = (PropertyUpdateRequest propertyUpdateRequest) =>
            {
                // Prevent setting all values on the general channel
                if (IsPropertyAvailable(t => t.DisplayName) && DisplayName == "General")
                {
                    propertyUpdateRequest.CancelUpdate("Updating the general channel is not allowed.");
                }
            };

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            UpdateApiCallOverrideHandler = async (ApiCallRequest apiCallRequest) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                if (DisplayName == "General")
                {
                    apiCallRequest.CancelRequest("Updating the general channel is not allowed.");
                }

                return apiCallRequest;
            };

            // Check delete, block when needed 
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            DeleteApiCallOverrideHandler = async (ApiCallRequest apiCallRequest) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                if (DisplayName == "General")
                {
                    apiCallRequest.CancelRequest("Deleting the general channel is not allowed.");
                }

                return apiCallRequest;
            };

        }
        #endregion

        #region Properties
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public bool IsFavoriteByDefault { get => GetValue<bool>(); set => SetValue(value); }

        public string Email { get => GetValue<string>(); set => SetValue(value); }

        public DateTime CreatedDateTime { get => GetValue<DateTime>(); set => SetValue(value); }

        public TeamChannelMembershipType MembershipType { get => GetValue<TeamChannelMembershipType>(); set => SetValue(value); }

        public Uri WebUrl { get => GetValue<Uri>(); set => SetValue(value); }

        [GraphProperty("filesFolderWebUrl", Beta = true)]
        public Uri FilesFolderWebUrl { get => GetValue<Uri>(); set => SetValue(value); }

        [GraphProperty("tabs", Get = "teams/{Site.GroupId}/channels/{GraphId}/tabs?$expand=teamsApp")]
        public ITeamChannelTabCollection Tabs { get => GetModelCollectionValue<ITeamChannelTabCollection>(); }

        [GraphProperty("messages", Get = "teams/{Site.GroupId}/channels/{GraphId}/messages", Beta = true)]
        public ITeamChatMessageCollection Messages { get => GetModelCollectionValue<ITeamChatMessageCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (string)value; }
        #endregion

        #region Methods
        public async Task<IFolder> GetFilesFolderAsync(params Expression<Func<IFolder, object>>[] expressions)
        {
            var apiCall = new ApiCall("teams/{Site.GroupId}/channels/{GraphId}/filesfolder", ApiType.Graph);
            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

            if (json.TryGetProperty("id", out JsonElement driveItemId))
            {
                var folderUniqueId = DriveHelper.DecodeDriveItemId(driveItemId.GetString());

                if (json.TryGetProperty("parentReference", out JsonElement parentReference) && parentReference.TryGetProperty("driveId", out JsonElement driveId))
                {
                    (Guid siteId, Guid webId, _) = DriveHelper.DecodeDriveId(driveId.GetString());

                    if (PnPContext.Site.Id == siteId && PnPContext.Web.Id == webId)
                    {
                        return await PnPContext.Web.GetFolderByIdAsync(folderUniqueId, expressions).ConfigureAwait(false);
                    }
                    else
                    {
                        // The files live in another site collection because channel is a private or shared one
                        if (json.TryGetProperty("webUrl", out JsonElement webUrl))
                        {
                            // drop the library and folder names to get the web url:
                            // https://microsoft.sharepoint.com/teams/ExtensibilityandFundamentals/Shared%20Documents/Documentation%20Planning means
                            // https://microsoft.sharepoint.com/teams/ExtensibilityandFundamentals

                            string contextUrl = "";

                            var webUrlSplit = webUrl.ToString().Split(new char[] { '/' }, StringSplitOptions.None);

                            for (int i = 0; i < webUrlSplit.Length - 2; i++)
                            {
                                contextUrl = contextUrl + webUrlSplit[i] + "/";
                            }

                            var newContext = await PnPContext.CloneAsync(new Uri(contextUrl)).ConfigureAwait(false);
                            return await newContext.Web.GetFolderByIdAsync(folderUniqueId, expressions).ConfigureAwait(false);
                        }
                    }
                }
            }

            return null;
        }

        public IFolder GetFilesFolder(params Expression<Func<IFolder, object>>[] expressions)
        {
            return GetFilesFolderAsync(expressions).GetAwaiter().GetResult();
        }
        #endregion

    }
}
