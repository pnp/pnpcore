using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.Teams
{
    internal static class TeamCreator
    {
        internal static async Task<PnPContext> CreateTeamForGroupAsync(PnPContext context, TeamForGroupOptions teamToCreate, TeamCreationOptions creationOptions = null)
        {
            // Ensure the needed creation options are configured
            creationOptions = EnsureTeamCreationOptions(creationOptions);

            // Construct the request body
            var body = BuildTeamCreationBody(teamToCreate);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            bool isProvisioningComplete = false;
            bool teamAlreadyExisted = false;
            var retryAttempt = 1;
            Exception lastException = null;
            do
            {
                if (retryAttempt > 1)
                {
                    context.Logger.LogDebug($"Elapsed: {stopwatch.Elapsed:mm\\:ss\\.fff} | Waiting {creationOptions.WaitAfterStatusCheck.Value} seconds");

                    await context.WaitAsync(TimeSpan.FromSeconds(creationOptions.WaitAfterStatusCheck.Value)).ConfigureAwait(false);
                }

                // Add the Team to the current group
                try
                {
                    await (context.Web as Web).RawRequestAsync(new ApiCall($"groups/{teamToCreate.GroupId}/team", ApiType.Graph, body), HttpMethod.Put).ConfigureAwait(false);
                    isProvisioningComplete = true;
                }
                catch (Exception ex)
                {
                    // Log and eat exception here
                    context.Logger.LogWarning(PnPCoreAdminResources.Log_Warning_ExceptionWhileCreatingTeamForGroup, teamToCreate.GroupId, ex.ToString());
                    lastException = ex;

                    // Leave the retry loop if the Team already existed
                    if ((ex is MicrosoftGraphServiceException graphServiceException) && graphServiceException.Error is MicrosoftGraphError microsoftGraphError)
                    {
                        // Conflict: means the Team already exists
                        if (microsoftGraphError.HttpResponseCode == 409)
                        {
                            teamAlreadyExisted = true;
                        }
                    }                    
                }

                retryAttempt++;
            }
            while (!(isProvisioningComplete || teamAlreadyExisted) && retryAttempt <= creationOptions.MaxStatusChecks);

            stopwatch.Stop();

            context.Logger.LogDebug($"Elapsed: {stopwatch.Elapsed:mm\\:ss\\.fff} | Finished");

            if (isProvisioningComplete || teamAlreadyExisted)
            {
                return await GetPnPContextWithTeamAsync(context, teamToCreate.GroupId).ConfigureAwait(false);
            }
            else
            {
                throw new MicrosoftGraphServiceException(string.Format(PnPCoreAdminResources.Exception_TeamCreation, teamToCreate.GroupId), lastException);
            }            
        }

        private static string BuildTeamCreationBody(TeamOptions options)
        {
            return JsonSerializer.Serialize(options, PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase);
        }

        private static async Task<PnPContext> GetPnPContextWithTeamAsync(PnPContext context, Guid groupId)
        {
            // Ensure the group id property was loaded
            await context.Site.EnsurePropertiesAsync(p => p.GroupId).ConfigureAwait(false);

            if (context.Site.GroupId == groupId)
            {
                // the passed context was for the correct group...only loading the team is sufficient
                await context.Team.LoadAsync().ConfigureAwait(false);
                return context;
            }
            else
            {
                // we need to clone the context
                var newContext = await context.CloneAsync(groupId).ConfigureAwait(false);
                await newContext.Team.LoadAsync().ConfigureAwait(false);
                return newContext;
            }
        }

        private static TeamCreationOptions EnsureTeamCreationOptions(TeamCreationOptions creationOptions)
        {
            if (creationOptions == null)
            {
                creationOptions = new TeamCreationOptions();
            }

            if (!creationOptions.MaxStatusChecks.HasValue)
            {
                creationOptions.MaxStatusChecks = 10;
            }

            if (!creationOptions.WaitAfterStatusCheck.HasValue)
            {
                creationOptions.WaitAfterStatusCheck = 30;
            }

            return creationOptions;
        }
    }
}
