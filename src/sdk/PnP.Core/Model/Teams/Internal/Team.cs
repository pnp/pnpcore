using PnP.Core.Services;
using System;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

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
                    api.CancelRequest("There is no Microsoft 365 group attached to the current site");
                }

                return api;
            };

        }

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

                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = false });

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

    }
}
