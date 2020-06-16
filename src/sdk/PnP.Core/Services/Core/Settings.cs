using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace PnP.Core.Services
{
    /// <summary>
    /// PnP Core SDK settings class
    /// </summary>
    public class Settings : ISettings
    {
        private const string UserAgentPrefix = "NONISV|SharePointPnP|PnPCoreSDK";
        private readonly ILogger logger;
        private readonly IConfiguration config;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="log">Connected logger</param>
        /// <param name="configuration">Configuration that was loaded</param>
        public Settings(ILogger<Settings> log, IConfiguration configuration)
        {
            logger = log;
            config = configuration;

            VersionTag = GetVersionTag();
            DisableTelemetry = GetDisableTelemetry();
            
            GraphFirst = GetGraphFirst();
            GraphAlwaysUseBeta = GetGraphAlwaysUseBeta();
            GraphCanUseBeta = GetGraphCanUseBeta();

            HttpUserAgent = GetHttpUserAgent();

            HttpSharePointRestUseRetryAfterHeader = GetHttpSharePointRestUseRetryAfterHeader();
            HttpSharePointRestMaxRetries = GetHttpSharePointRestMaxRetries();
            HttpSharePointRestDelayInSeconds = GetHttpSharePointRestDelayInSeconds();
            HttpSharePointRestUseIncrementalDelay = GetHttpSharePointRestIncrementalDelay();

            HttpMicrosoftGraphUseRetryAfterHeader = GetHttpMicrosoftGraphUseRetryAfterHeader();
            HttpMicrosoftGraphMaxRetries = GetHttpMicrosoftGraphMaxRetries();
            HttpMicrosoftGraphDelayInSeconds = GetHttpMicrosoftGraphDelayInSeconds();
            HttpMicrosoftGraphUseIncrementalDelay = GetHttpMicrosoftGraphIncrementalDelay();
        }

        /// <summary>
        /// Turns on/off telemetry, can be customized via configuration. Defaults to false.
        /// </summary>
        public bool DisableTelemetry { get; }

        #region Http request settings

        /// <summary>
        /// User agent value, can be customized via configuration 
        /// </summary>
        public string HttpUserAgent { get; }

        /// <summary>
        /// Use the Retry-After header for calculating the delay in case of a retry. Defaults to false
        /// </summary>
        public bool HttpSharePointRestUseRetryAfterHeader { get; }

        /// <summary>
        /// When not using retry-after, how many times can a retry be made. Defaults to 10
        /// </summary>
        public int HttpSharePointRestMaxRetries { get; }

        /// <summary>
        /// How many seconds to wait for the next retry attempt. Defaults to 3
        /// </summary>
        public int HttpSharePointRestDelayInSeconds { get; }

        /// <summary>
        /// Use an incremental strategy for the delay: each retry doubles the previous delay time. Defaults to true
        /// </summary>
        public bool HttpSharePointRestUseIncrementalDelay { get; }

        /// <summary>
        /// Use the Retry-After header for calculating the delay in case of a retry. Defaults to true
        /// </summary>
        public bool HttpMicrosoftGraphUseRetryAfterHeader { get; }

        /// <summary>
        /// When not using retry-after, how many times can a retry be made. Defaults to 10
        /// </summary>
        public int HttpMicrosoftGraphMaxRetries { get; }

        /// <summary>
        /// How many seconds to wait for the next retry attempt. Defaults to 3
        /// </summary>
        public int HttpMicrosoftGraphDelayInSeconds { get; }

        /// <summary>
        /// Use an incremental strategy for the delay: each retry doubles the previous delay time. Defaults to true
        /// </summary>
        public bool HttpMicrosoftGraphUseIncrementalDelay { get; }

        #endregion

        #region PnP Context settings

        /// <summary>
        /// Turns on/off the Microsoft Graph first behavior, can be customized via configuration. Defaults to true.
        /// </summary>
        public bool GraphFirst { get; }

        /// <summary>
        /// Always uses the Microsoft Graph Beta api's, can be customized via configuration. Defaults to false.
        /// </summary>
        public bool GraphAlwaysUseBeta { get; }

        /// <summary>
        /// Turns on/off the option to use Microsoft Graph Beta api's, can be customized via configuration 
        /// </summary>
        public bool GraphCanUseBeta { get; }

        #endregion

        #region Internal only settings (supporting, they cannot be assigned from configuration)
        /// <summary>
        /// Version tag used in telemetry
        /// </summary>
        public string VersionTag { get; }

        /// <summary>
        /// AAD tenant id, used for telemetry purposes. Can be customized via configuration
        /// </summary>
        public Guid AADTenantId { get; set; }

        #endregion

        #region Settings implementation

        private bool GetHttpSharePointRestUseRetryAfterHeader()
        {
            var useRetryAfter = config.GetValue<bool>("PnPCore:HttpRequests:SharePointRest:UseRetryAfterHeader", false);
            logger.LogInformation($"Using telemetry setting from configuration. SharePointRest:UseRetryAfterHeader: {useRetryAfter}");
            return useRetryAfter;
        }

        private bool GetHttpMicrosoftGraphUseRetryAfterHeader()
        {
            var useRetryAfter = config.GetValue<bool>("PnPCore:HttpRequests:MicrosoftGraph:UseRetryAfterHeader", true);
            logger.LogInformation($"Using telemetry setting from configuration. MicrosoftGraph:UseRetryAfterHeader: {useRetryAfter}");
            return useRetryAfter;
        }

        private int GetHttpSharePointRestMaxRetries()
        {
            var maxRetries = config.GetValue<int>("PnPCore:HttpRequests:SharePointRest:MaxRetries", 10);
            logger.LogInformation($"Using telemetry setting from configuration. SharePointRest:MaxRetries: {maxRetries}");
            return maxRetries;
        }

        private int GetHttpMicrosoftGraphMaxRetries()
        {
            var maxRetries = config.GetValue<int>("PnPCore:HttpRequests:MicrosoftGraph:MaxRetries", 10);
            logger.LogInformation($"Using telemetry setting from configuration. MicrosoftGraph:MaxRetries: {maxRetries}");
            return maxRetries;
        }

        private int GetHttpSharePointRestDelayInSeconds()
        {
            var delayInSeconds = config.GetValue<int>("PnPCore:HttpRequests:SharePointRest:DelayInSeconds", 3);
            logger.LogInformation($"Using telemetry setting from configuration. SharePointRest:DelayInSeconds: {delayInSeconds}");
            return delayInSeconds;
        }

        private int GetHttpMicrosoftGraphDelayInSeconds()
        {
            var delayInSeconds = config.GetValue<int>("PnPCore:HttpRequests:MicrosoftGraph:DelayInSeconds", 3);
            logger.LogInformation($"Using telemetry setting from configuration. MicrosoftGraph:DelayInSeconds: {delayInSeconds}");
            return delayInSeconds;
        }

        private bool GetHttpSharePointRestIncrementalDelay()
        {
            var incrementalDelay = config.GetValue<bool>("PnPCore:HttpRequests:SharePointRest:IncrementalDelay", true);
            logger.LogInformation($"Using telemetry setting from configuration. SharePointRest:IncrementalDelay: {incrementalDelay}");
            return incrementalDelay;
        }

        private bool GetHttpMicrosoftGraphIncrementalDelay()
        {
            var incrementalDelay = config.GetValue<bool>("PnPCore:HttpRequests:MicrosoftGraph:IncrementalDelay", true);
            logger.LogInformation($"Using telemetry setting from configuration. MicrosoftGraph:IncrementalDelay: {incrementalDelay}");
            return incrementalDelay;
        }

        private bool GetGraphFirst()
        {
            var graphFirst = config.GetValue<bool>("PnPCore:PnPContext:GraphFirst", true);
            logger.LogInformation($"Using telemetry setting from configuration. GraphFirst: {graphFirst}");
            return graphFirst;
        }

        private bool GetGraphAlwaysUseBeta()
        {
            var graphAlwaysUsesBeta = config.GetValue<bool>("PnPCore:PnPContext:GraphAlwaysUseBeta", false);
            logger.LogInformation($"Using telemetry setting from configuration. GraphAlwaysUseBeta: {graphAlwaysUsesBeta}");
            return graphAlwaysUsesBeta;
        }

        private bool GetGraphCanUseBeta()
        {
            var graphCanUseBeta = config.GetValue<bool>("PnPCore:PnPContext:GraphCanUseBeta", true);
            logger.LogInformation($"Using telemetry setting from configuration. GraphCanUseBeta: {graphCanUseBeta}");
            return graphCanUseBeta;
        }

        private bool GetDisableTelemetry()
        {
            var disableTelemetry = config.GetValue<bool>("PnPCore:DisableTelemetry", false);
            logger.LogInformation($"Using telemetry setting from configuration. Telemetry disabled: {disableTelemetry}");
            return disableTelemetry;
        }

        private string GetHttpUserAgent()
        {
            var userAgentFromConfig = config.GetValue<string>("PnPCore:HttpRequests:UserAgent");
            if (!string.IsNullOrEmpty(userAgentFromConfig))
            {
                logger.LogInformation($"Using user-agent value {userAgentFromConfig} provided by configuration");
                return userAgentFromConfig;
            }
            else
            {
                Assembly coreAssembly = Assembly.GetExecutingAssembly();
                var userAgentDefault = $"{UserAgentPrefix}/{((AssemblyFileVersionAttribute)coreAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version}";
                logger.LogInformation($"Using default user-agent value {userAgentDefault}");
                return userAgentDefault;
            }
        }

        private string GetVersionTag()
        {
            Assembly coreAssembly = Assembly.GetExecutingAssembly();
            var versionTag = $"PnPSDK:{((AssemblyFileVersionAttribute)coreAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version.Split('.')[2]}";
            logger.LogInformation($"Using version tag value {versionTag}");
            return versionTag;
        }

        #endregion
    }
}
