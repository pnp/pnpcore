using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal class TeamAsyncOperation : BaseDataModel<ITeamAsyncOperation>, ITeamAsyncOperation
    {
        private const int waitTimeInSeconds = 5;
        private const int maxRetries = 50;

        internal TeamAsyncOperation(string location, PnPContext context)
        {
            Location = location;
            PnPContext = context;
        }

        public string Location { get => GetValue<string>(); set => SetValue(value); }

        public async Task WaitForCompletionAsync()
        {
            bool wereDone = false;
            int retries = 0;

            while (!wereDone)
            {
                if (retries >= maxRetries)
                {
                    throw new ServiceException(ErrorType.TeamsAsyncOperationError, 0, 
                        string.Format(PnPCoreResources.Exception_TeamsAsyncOperationError_MaxRetries, maxRetries, Location));
                }

                var apiCall = new ApiCall(Location.ToString(), ApiType.Graph);
                var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(response.Json))
                {
                    //https://docs.microsoft.com/en-us/graph/api/resources/teamsasyncoperation?view=graph-rest-beta
                    var document = JsonSerializer.Deserialize<JsonElement>(response.Json);
                    if (document.TryGetProperty("status", out JsonElement status))
                    {
                        var statusValue = status.GetString();
                        if (statusValue.Equals("succeeded", StringComparison.InvariantCultureIgnoreCase) ||
                            statusValue.Equals("invalid", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Log.LogDebug($"Operation {Location} completed");
                            wereDone = true;
                        }
                        else if (statusValue.Equals("failed", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (document.TryGetProperty("error", out JsonElement error))
                            {
                                throw new ServiceException(ErrorType.TeamsAsyncOperationError, 0, error.ToString());
                            }
                        }
                        else
                        {
                            retries++;
                            Log.LogInformation($"Operation {Location} not ready yet. Status = {statusValue}. Waiting for {waitTimeInSeconds} seconds, attempt number {retries}");
                            Thread.Sleep(waitTimeInSeconds * 1000);
                        }
                    }
                }
            }
        }

        public void WaitForCompletion()
        {
            WaitForCompletionAsync().GetAwaiter().GetResult();
        }

    }
}
