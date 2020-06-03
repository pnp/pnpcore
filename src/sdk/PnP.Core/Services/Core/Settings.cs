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
            UserAgent = GetUserAgent();
            DisableTelemetry = GetDisableTelemetry();
            GraphFirst = GetGraphFirst();
            GraphAlwaysUseBeta = GetGraphAlwaysUseBeta();
            GraphCanUseBeta = GetGraphCanUseBeta();
        }

        /// <summary>
        /// Version tag used in telemetry
        /// </summary>
        public string VersionTag { get; }

        /// <summary>
        /// User agent value, can be customized via configuration 
        /// </summary>
        public string UserAgent { get; }

        /// <summary>
        /// Turns on/off telemetry, can be customized via configuration. Defaults to false.
        /// </summary>
        public bool DisableTelemetry { get; }

        /// <summary>
        /// AAD tenant id, used for telemetry purposes. Can be customized via configuration
        /// </summary>
        public Guid AADTenantId { get; set; }

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

        private string GetUserAgent()
        {
            var userAgentFromConfig = config.GetValue<string>("PnPCore:UserAgent");
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
    }
}
