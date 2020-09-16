using Microsoft.Extensions.Options;
using System.Net.Http;

namespace PnP.Core.Services
{
    /// <summary>
    /// Retry handler for SharePoint REST requests
    /// </summary>
    internal class SharePointRestRetryHandler: RetryHandlerBase
    {
        #region Construction
        public SharePointRestRetryHandler(IOptions<PnPGlobalSettingsOptions> globalSettings) : base(globalSettings.Value)
        {
            Configure();
        }

        public SharePointRestRetryHandler(HttpMessageHandler innerHandler, IOptions<PnPGlobalSettingsOptions> globalSettings) : base(innerHandler, globalSettings?.Value)
        {
            Configure();
        }
        #endregion

        private void Configure()
        {
            if (GlobalSettings != null)
            {
                UseRetryAfterHeader = GlobalSettings.HttpSharePointRestUseRetryAfterHeader;
                MaxRetries = GlobalSettings.HttpSharePointRestMaxRetries;
                DelayInSeconds = GlobalSettings.HttpSharePointRestDelayInSeconds;
                IncrementalDelay = GlobalSettings.HttpSharePointRestUseIncrementalDelay;
            }
        }
    }
}
