using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    [GraphType]
    internal class TeamAsyncOperation : BaseDataModel<ITeamAsyncOperation>, ITeamAsyncOperation
    {
        internal int WaitTimeInSeconds { get; set; } = 5;
        internal int MaxRetries { get; set; } = 50;

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
                if (retries >= MaxRetries)
                {
                    throw new ServiceException(ErrorType.TeamsAsyncOperationError, 0,
                        string.Format(PnPCoreResources.Exception_TeamsAsyncOperationError_MaxRetries, MaxRetries, Location));
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
                            Log.LogDebug(PnPCoreResources.Log_Debug_OperationCompleted, Location);
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
                            Log.LogInformation(PnPCoreResources.Log_Information_OperationNotReady, Location, statusValue, WaitTimeInSeconds, retries);
                            await Task.Delay(TimeSpan.FromSeconds(WaitTimeInSeconds)).ConfigureAwait(false);
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
