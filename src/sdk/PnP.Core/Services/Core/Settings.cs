using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace PnP.Core.Services
{
    public class Settings : ISettings
    {
        private const string UserAgentPrefix = "NONISV|SharePointPnP|PnPdotNet";
        private readonly ILogger logger;
        private readonly IConfiguration config;

        public Settings(ILogger<Settings> log, IConfiguration configuration)
        {
            logger = log;
            config = configuration;

            VersionTag = GetVersionTag();
            UserAgent = GetUserAgent();
        }

        public string VersionTag { get; }

        public string UserAgent { get; }

        private string GetUserAgent()
        {
            var userAgentFromConfig = config.GetValue<string>("PnPDotNet:UserAgent");
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
            var versionTag = $"PnPNet:{((AssemblyFileVersionAttribute)coreAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version.Split('.')[2]}";
            logger.LogInformation($"Using version tag value {versionTag}");
            return versionTag;
        }
    }
}
