using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace PnP.Core.Services
{
    /// <summary>
    /// PnP Core SDK settings class
    /// </summary>
    public class PnPGlobalSettingsOptions
    {
        private const string UserAgentPrefix = "NONISV|SharePointPnP|PnPCoreSDK";
        private readonly ILogger logger;

        /// <summary>
        /// Turns on/off telemetry, can be customized via configuration. Defaults to false.
        /// </summary>
        public bool DisableTelemetry { get; set; }

        #region Http request settings

        private string httpUserAgent;

        /// <summary>
        /// User agent value, can be customized via configuration 
        /// </summary>
        public string HttpUserAgent
        {
            get
            {
                if (string.IsNullOrEmpty(httpUserAgent))
                {
                    Assembly coreAssembly = Assembly.GetExecutingAssembly();
                    var userAgentDefault = $"{UserAgentPrefix}/{((AssemblyFileVersionAttribute)coreAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version}";
                    // logger.LogInformation(PnPCoreResources.Log_Information_UsingDefaultUserAgent, userAgentDefault);
                    httpUserAgent = userAgentDefault;
                }

                return httpUserAgent;
            }
            internal set
            {
                // logger.LogInformation(PnPCoreResources.Log_Information_UsingUserAgent, httpUserAgent);
                httpUserAgent = value;
            }
        }

        /// <summary>
        /// Use the Retry-After header for calculating the delay in case of a retry. Defaults to false
        /// </summary>
        public bool HttpSharePointRestUseRetryAfterHeader { get; set; }

        /// <summary>
        /// When not using retry-after, how many times can a retry be made. Defaults to 10
        /// </summary>
        public int HttpSharePointRestMaxRetries { get; set; } = 10;

        /// <summary>
        /// How many seconds to wait for the next retry attempt. Defaults to 3
        /// </summary>
        public int HttpSharePointRestDelayInSeconds { get; set; } = 3;

        /// <summary>
        /// Use an incremental strategy for the delay: each retry doubles the previous delay time. Defaults to true
        /// </summary>
        public bool HttpSharePointRestUseIncrementalDelay { get; set; } = true;

        /// <summary>
        /// Use the Retry-After header for calculating the delay in case of a retry. Defaults to true
        /// </summary>
        public bool HttpMicrosoftGraphUseRetryAfterHeader { get; set; } = true;

        /// <summary>
        /// When not using retry-after, how many times can a retry be made. Defaults to 10
        /// </summary>
        public int HttpMicrosoftGraphMaxRetries { get; set; } = 10;

        /// <summary>
        /// How many seconds to wait for the next retry attempt. Defaults to 3
        /// </summary>
        public int HttpMicrosoftGraphDelayInSeconds { get; set; } = 3;

        /// <summary>
        /// Use an incremental strategy for the delay: each retry doubles the previous delay time. Defaults to true
        /// </summary>
        public bool HttpMicrosoftGraphUseIncrementalDelay { get; set; } = true;

        #endregion

        #region Internal only settings (supporting, they cannot be assigned from configuration)

        private string versionTag;

        /// <summary>
        /// Version tag used in telemetry
        /// </summary>
        public string VersionTag
        {
            get
            {
                if (string.IsNullOrEmpty(versionTag))
                {
                    versionTag = GetVersionTag();
                }

                return versionTag;
            }
        }

        /// <summary>
        /// AAD tenant id, used for telemetry purposes. Can be customized via configuration
        /// </summary>
        public Guid AADTenantId { get; set; }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public PnPGlobalSettingsOptions()
        {

        }

        /// <summary>
        /// Custom constructor
        /// </summary>
        /// <param name="log">Connected logger</param>
        public PnPGlobalSettingsOptions(ILogger<PnPGlobalSettingsOptions> log)
        {
            logger = log;
        }

        private static string GetVersionTag()
        {
            Assembly coreAssembly = Assembly.GetExecutingAssembly();
            var versionTag = $"PnPSDK:{((AssemblyFileVersionAttribute)coreAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version.Split('.')[2]}";
            // logger.LogInformation(PnPCoreResources.Log_Information_VersionTag, versionTag);
            return versionTag;
        }
    }
}
