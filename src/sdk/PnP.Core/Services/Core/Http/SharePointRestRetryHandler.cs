using System.Net.Http;

namespace PnP.Core.Services
{
    /// <summary>
    /// Retry handler for SharePoint REST requests
    /// </summary>
    internal class SharePointRestRetryHandler: RetryHandlerBase
    {
        #region Construction
        public SharePointRestRetryHandler(ISettings settingsClient): base(settingsClient)
        {
            Configure();
        }

        public SharePointRestRetryHandler(HttpMessageHandler innerHandler, ISettings settingsClient) : base(innerHandler, settingsClient)
        {
            Configure();
        }
        #endregion

        private void Configure()
        {
            if (SettingsClient != null)
            {
                UseRetryAfterHeader = SettingsClient.HttpSharePointRestUseRetryAfterHeader;
                MaxRetries = SettingsClient.HttpSharePointRestMaxRetries;
                DelayInSeconds = SettingsClient.HttpSharePointRestDelayInSeconds;
                IncrementalDelay = SettingsClient.HttpSharePointRestUseIncrementalDelay;
            }
        }
    }
}
