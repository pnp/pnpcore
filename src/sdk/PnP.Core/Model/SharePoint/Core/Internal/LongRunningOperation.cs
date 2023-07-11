using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal class LongRunningOperation : BaseDataModel<ILongRunningOperation>, ILongRunningOperation
    {
        internal static int DefaultWaitTimeInSeconds = 5;

        internal LongRunningOperation(string location, PnPContext context)
        {
            Location = location;
            PnPContext = context;
        }

        public string Location { get => GetValue<string>(); set => SetValue(value); }

        public async Task WaitForCompletionAsync(LongRunningOperationOptions options = null)
        {
            bool wereDone = false;
            int retries = 0;

            options = EnsureOptions(options);

            while (!wereDone)
            {
                if (retries >= options.MaxRetries)
                {
                    throw new ServiceException(ErrorType.TeamsAsyncOperationError, 0,
                        string.Format(PnPCoreResources.Exception_AsyncOperationError_MaxRetries, options.MaxRetries, Location));
                }

                string graphUrl;
                if (PnPContext.Environment == Microsoft365Environment.Custom)
                {
                    graphUrl = $"https://{PnPContext.MicrosoftGraphAuthority}/";
                }
                else
                {
                    graphUrl = $"https://{CloudManager.GetMicrosoftGraphAuthority(PnPContext.Environment.Value)}/";
                }

                var apiCall = new ApiCall(Location.ToString().Replace($"{graphUrl}{PnPConstants.GraphV1Endpoint}/", ""), ApiType.Graph);
                var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(response.Json))
                {
                    //https://docs.microsoft.com/en-us/graph/api/richlongrunningoperation-get?view=graph-rest-1.0&tabs=http
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
                            Log.LogInformation(PnPCoreResources.Log_Information_OperationNotReady, Location, statusValue, options.WaitTimeInSeconds.Value, retries);
                            await Task.Delay(TimeSpan.FromSeconds(options.WaitTimeInSeconds.Value)).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        public void WaitForCompletion(LongRunningOperationOptions options = null)
        {
            WaitForCompletionAsync(options).GetAwaiter().GetResult();
        }

        private static LongRunningOperationOptions EnsureOptions(LongRunningOperationOptions options)
        {
            if (options == null)
            {
                options = new LongRunningOperationOptions();
            }

            if (!options.WaitTimeInSeconds.HasValue)
            {
                options.WaitTimeInSeconds = DefaultWaitTimeInSeconds;
            }

            if (options.WaitTimeInSeconds < 1)
            {
                options.WaitTimeInSeconds = DefaultWaitTimeInSeconds;
            }

            if (!options.MaxRetries.HasValue)
            {
                options.MaxRetries = int.MaxValue;
            }

            return options;
        }
    }
}
