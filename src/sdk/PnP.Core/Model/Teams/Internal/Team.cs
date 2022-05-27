using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = "teams/{Site.GroupId}", LinqGet = "teams")]
    internal sealed class Team : BaseDataModel<ITeam>, ITeam
    {
        #region Construction
        public Team()
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            GetApiCallOverrideHandler = async (ApiCallRequest api) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                if (!PnPContext.Site.IsPropertyAvailable(p => p.GroupId) || PnPContext.Site.GroupId == Guid.Empty)
                {
                    api.CancelRequest("There is no Microsoft 365 group attached to the current site");
                }

                return api;
            };

        }
        #endregion

        #region Properties
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public string InternalId { get => GetValue<string>(); set => SetValue(value); }

        public string Classification { get => GetValue<string>(); set => SetValue(value); }

        public TeamSpecialization Specialization { get => GetValue<TeamSpecialization>(); set => SetValue(value); }

        public TeamVisibility Visibility { get => GetValue<TeamVisibility>(); set => SetValue(value); }

        public Uri WebUrl { get => GetValue<Uri>(); set => SetValue(value); }

        public bool IsArchived { get => GetValue<bool>(); set => SetValue(value); }

        public ITeamMembersSettings MemberSettings { get => GetModelValue<ITeamMembersSettings>(); }

        public ITeamGuestSettings GuestSettings { get => GetModelValue<ITeamGuestSettings>(); }

        public ITeamMessagingSettings MessagingSettings { get => GetModelValue<ITeamMessagingSettings>(); }

        public ITeamFunSettings FunSettings { get => GetModelValue<ITeamFunSettings>(); }

        public ITeamDiscoverySettings DiscoverySettings { get => GetModelValue<ITeamDiscoverySettings>(); }

        public ITeamClassSettings ClassSettings { get => GetModelValue<ITeamClassSettings>(); }

        [GraphProperty("primaryChannel", Get = "teams/{Site.GroupId}/primarychannel")]
        public ITeamChannel PrimaryChannel { get => GetModelValue<ITeamChannel>(); }

        [GraphProperty("channels", Get = "teams/{Site.GroupId}/channels")]
        public ITeamChannelCollection Channels { get => GetModelCollectionValue<ITeamChannelCollection>(); }

        [GraphProperty("installedApps", Get = "teams/{Site.GroupId}/installedapps?$expand=TeamsApp")]
        public ITeamAppCollection InstalledApps { get => GetModelCollectionValue<ITeamAppCollection>(); }

        [GraphProperty("owners", Get = "groups/{Site.GroupId}/owners")]
        public IGraphUserCollection Owners { get => GetModelCollectionValue<IGraphUserCollection>(); }

        [GraphProperty("members", Get = "groups/{Site.GroupId}/members")]
        public IGraphUserCollection Members { get => GetModelCollectionValue<IGraphUserCollection>(); }

        [GraphProperty("tags", Get = "teams/{Site.GroupId}/tags", Beta = true)]
        public ITeamTagCollection Tags { get => GetModelCollectionValue<ITeamTagCollection>(); }

        [GraphProperty("events", Get = "groups/{Site.GroupId}/events")]
        public IGraphEventCollection Events { get => GetModelCollectionValue<IGraphEventCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }
        #endregion

        #region Extension methods
        public async Task<ITeamAsyncOperation> ArchiveAsync()
        {
            return await ArchiveAsync(false).ConfigureAwait(false);
        }

        public ITeamAsyncOperation Archive()
        {
            return ArchiveAsync().GetAwaiter().GetResult();
        }

        public async Task<ITeamAsyncOperation> ArchiveAsync(bool setSPOSiteReadOnlyForMembers)
        {
            if (Requested)
            {

                dynamic body = new ExpandoObject();
                body.shouldSetSpoSiteReadOnlyForMembers = setSPOSiteReadOnlyForMembers;

                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_WriteIndentedFalse);

                var apiCall = new ApiCall($"teams/{Id}/archive", ApiType.Graph, bodyContent);

                var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted && response.Headers != null && response.Headers.ContainsKey("Location"))
                {
                    // The archiving operation is in progress, already set the Team IsArchived flag to true
                    (this as ITeam).SetSystemProperty(p => p.IsArchived, true);

                    // we get back a url to request a teamsAsyncOperation (https://docs.microsoft.com/en-us/graph/api/resources/teamsasyncoperation?view=graph-rest-beta)
                    //return response.Headers["Location"];
                    return new TeamAsyncOperation(response.Headers["Location"], PnPContext);
                }
            }

            return null;
        }

        public ITeamAsyncOperation Archive(bool setSPOSiteReadOnlyForMembers)
        {
            return ArchiveAsync(setSPOSiteReadOnlyForMembers).GetAwaiter().GetResult();
        }

        public async Task<ITeamAsyncOperation> UnarchiveAsync()
        {
            if (Requested)
            {
                // We're sending in a "fake" body as otherwise the graph batch validation logic blocks this request 
                // with error: "Write request id : 1 does not contain Content-Type header or body."
                var apiCall = new ApiCall($"teams/{Id}/unarchive", ApiType.Graph, "{\"bla\":\"bla\"}");

                var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted && response.Headers != null && response.Headers.ContainsKey("Location"))
                {
                    // The archiving operation is in progress, already set the Team IsArchived flag to true
                    (this as ITeam).SetSystemProperty(p => p.IsArchived, false);

                    // we get back a url to request a teamsAsyncOperation (https://docs.microsoft.com/en-us/graph/api/resources/teamsasyncoperation?view=graph-rest-beta)
                    //return response.Headers["Location"];
                    return new TeamAsyncOperation(response.Headers["Location"], PnPContext);
                }
            }

            return null;
        }

        public ITeamAsyncOperation Unarchive()
        {
            return UnarchiveAsync().GetAwaiter().GetResult();
        }
        #endregion
    }
}
