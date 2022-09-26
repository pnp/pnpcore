using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Services.Core;

namespace PnP.Core.Services
{
    /// <summary>
    /// Retry handler for Microsoft Graph requests
    /// </summary>
    internal sealed class MicrosoftGraphRetryHandler : RetryHandlerBase
    {
        #region Construction
        public MicrosoftGraphRetryHandler(ILogger<RetryHandlerBase> log, IOptions<PnPGlobalSettingsOptions> globalSettings, EventHub eventHub, RateLimiter limiter) : base(log, globalSettings?.Value, eventHub, limiter)
        {
            Configure();
        }
        #endregion

        private void Configure()
        {
            if (GlobalSettings != null)
            {
                UseRetryAfterHeader = GlobalSettings.HttpMicrosoftGraphUseRetryAfterHeader;
                MaxRetries = GlobalSettings.HttpMicrosoftGraphMaxRetries;
                DelayInSeconds = GlobalSettings.HttpMicrosoftGraphDelayInSeconds;
                IncrementalDelay = GlobalSettings.HttpMicrosoftGraphUseIncrementalDelay;
            }
        }

    }
}
