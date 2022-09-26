using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Services.Core;

namespace PnP.Core.Services
{
    /// <summary>
    /// Retry handler for SharePoint REST requests
    /// </summary>
    internal sealed class SharePointRestRetryHandler : RetryHandlerBase
    {
        #region Construction
        public SharePointRestRetryHandler(ILogger<RetryHandlerBase> log, IOptions<PnPGlobalSettingsOptions> globalSettings, EventHub eventHub, RateLimiter limiter) : base(log, globalSettings?.Value, eventHub, limiter)
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
