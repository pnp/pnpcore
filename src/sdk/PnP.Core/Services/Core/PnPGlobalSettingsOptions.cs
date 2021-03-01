using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading;

namespace PnP.Core.Services
{
    /// <summary>
    /// PnP Core SDK settings class
    /// </summary>
    public class PnPGlobalSettingsOptions
    {
        private const string UserAgentPrefix = "NONISV|SharePointPnP|PnPCoreSDK";
        
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
        /// Max duration of a http request in seconds. Defaults to 100 seconds, set to value -1 for an infinite timeout
        /// </summary>
        public int HttpTimeout { get; set; } = 100;

        /// <summary>
        /// Use the Retry-After header for calculating the delay in case of a retry. Defaults to false
        /// </summary>
        public bool HttpSharePointRestUseRetryAfterHeader { get; set; }

        /// <summary>
        /// Defines the default page size for querying items in SharePoint REST
        /// </summary>
        public int HttpSharePointRestDefaultPageSize { get; set; } = 100;

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
        /// Connected logger
        /// </summary>
        internal ILogger Logger { get; set; }

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
            Logger = log;
        }

        private static string GetVersionTag()
        {
            Assembly coreAssembly = Assembly.GetExecutingAssembly();
            var version = ((AssemblyFileVersionAttribute)coreAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version.Split('.');
            var versionTag = $"PnPSDK:{version[0]}.{version[1]}";
            return versionTag;
        }

        internal TimeSpan GetHttpTimeout()
        {
            if (HttpTimeout == -1)
            {
                return Timeout.InfiniteTimeSpan;
            }
            else
            {
                return new TimeSpan(0, 0, HttpTimeout);
            }
        }

    }
}
