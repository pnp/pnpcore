using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// PnP Core SDK settings
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Turns on/off telemetry, can be customized via configuration. Defaults to false.
        /// </summary>
        bool DisableTelemetry { get; }

        // PnP Context settings

        /// <summary>
        /// Turns on/off the Microsoft Graph first behavior, can be customized via configuration. Defaults to true.
        /// </summary>
        bool GraphFirst { get; }

        /// <summary>
        /// Always uses the Microsoft Graph Beta api's, can be customized via configuration. Defaults to false.
        /// </summary>
        bool GraphAlwaysUseBeta { get; }

        /// <summary>
        /// Turns on/off the option to use Microsoft Graph Beta api's, can be customized via configuration 
        /// </summary>
        bool GraphCanUseBeta { get; }

        // Http request settings
        /// <summary>
        /// User agent value, can be customized via configuration 
        /// </summary>
        string HttpUserAgent { get; }

        // Retry settings

        /// <summary>
        /// Use the Retry-After header for calculating the delay in case of a retry. Defaults to false
        /// </summary>
        bool HttpSharePointRestUseRetryAfterHeader { get; }

        /// <summary>
        /// When not using retry-after, how many times can a retry be made. Defaults to 10
        /// </summary>
        int HttpSharePointRestMaxRetries { get; }

        /// <summary>
        /// How many seconds to wait for the next retry attempt. Defaults to 3
        /// </summary>
        int HttpSharePointRestDelayInSeconds { get; }

        /// <summary>
        /// Use an incremental strategy for the delay: each retry doubles the previous delay time. Defaults to true
        /// </summary>
        bool HttpSharePointRestUseIncrementalDelay { get; }

        /// <summary>
        /// Use the Retry-After header for calculating the delay in case of a retry. Defaults to true
        /// </summary>
        bool HttpMicrosoftGraphUseRetryAfterHeader { get; }

        /// <summary>
        /// When not using retry-after, how many times can a retry be made. Defaults to 10
        /// </summary>
        int HttpMicrosoftGraphMaxRetries { get; }

        /// <summary>
        /// How many seconds to wait for the next retry attempt. Defaults to 3
        /// </summary>
        int HttpMicrosoftGraphDelayInSeconds { get; }

        /// <summary>
        /// Use an incremental strategy for the delay: each retry doubles the previous delay time. Defaults to true
        /// </summary>
        bool HttpMicrosoftGraphUseIncrementalDelay { get; }

        // Internal only settings (supporting, they cannot be assigned from configuration)
        /// <summary>
        /// Version tag used in telemetry
        /// </summary>
        string VersionTag { get; }

        /// <summary>
        /// AAD tenant id, used for telemetry purposes. Can be customized via configuration
        /// </summary>
        Guid AADTenantId { get; set; }
    }
}
