using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PnP.Core.Services
{
    /// <summary>
    /// Retry handler for Microsoft Graph requests
    /// </summary>
    internal sealed class MicrosoftGraphRetryHandler : RetryHandlerBase
    {
        #region Construction
        public MicrosoftGraphRetryHandler(ILogger<RetryHandlerBase> log, IOptions<PnPGlobalSettingsOptions> globalSettings) : base(log, globalSettings?.Value)
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
