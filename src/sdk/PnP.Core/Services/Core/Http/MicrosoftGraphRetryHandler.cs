using Microsoft.Extensions.Options;

namespace PnP.Core.Services
{
    /// <summary>
    /// Retry handler for Microsoft Graph requests
    /// </summary>
    internal class MicrosoftGraphRetryHandler: RetryHandlerBase
    {
        #region Construction
        public MicrosoftGraphRetryHandler(IOptions<PnPGlobalSettingsOptions> globalSettings) : base(globalSettings.Value)
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
