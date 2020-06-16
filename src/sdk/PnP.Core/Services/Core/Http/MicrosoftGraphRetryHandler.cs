using System.Net.Http;

namespace PnP.Core.Services
{
    /// <summary>
    /// Retry handler for Microsoft Graph requests
    /// </summary>
    internal class MicrosoftGraphRetryHandler: RetryHandlerBase
    {
        #region Construction
        public MicrosoftGraphRetryHandler(ISettings settingsClient) : base(settingsClient)
        {
            Configure();
        }

        public MicrosoftGraphRetryHandler(HttpMessageHandler innerHandler, ISettings settingsClient) : base(innerHandler, settingsClient)
        {
            Configure();
        }
        #endregion

        private void Configure()
        {
            if (SettingsClient != null)
            {
                UseRetryAfterHeader = SettingsClient.HttpMicrosoftGraphUseRetryAfterHeader;
                MaxRetries = SettingsClient.HttpMicrosoftGraphMaxRetries;
                DelayInSeconds = SettingsClient.HttpMicrosoftGraphDelayInSeconds;
                IncrementalDelay = SettingsClient.HttpMicrosoftGraphUseIncrementalDelay;
            }
        }

    }
}
