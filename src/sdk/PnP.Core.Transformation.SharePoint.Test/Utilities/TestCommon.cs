using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using PnP.Core.Auth;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services.Builder.Configuration;
using File = System.IO.File;

namespace PnP.Core.Transformation.Test.Utilities
{
    public static class TestCommon
    {

        /// <summary>
        /// Name of the default test target site configuration
        /// </summary>
        internal static string TargetTestSite => "TargetTestSite";

        public static IConfigurationRoot GetConfigurationSettings()
        {
            // Define the test environment by: 
            // - Copying env.sample to env.txt  
            // - Putting the test environment name in env.txt ==> this should be same name as used in your settings file:
            //   When using appsettings.mine.json then you need to put mine as content in env.txt
            var environmentName = LoadTestEnvironment();

            if (string.IsNullOrEmpty(environmentName))
            {
                throw new Exception("Please ensure you've a env.txt file in the root of the test project. This file should contain the name of the test environment you want to use.");
            }

            // The settings file is stored in the root of the test project, no need to configure the file to be copied over the bin folder
            var jsonSettingsFile = Path.GetFullPath($"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}appsettings.{environmentName}.json");

            var configuration = new ConfigurationBuilder()
            .AddJsonFile(jsonSettingsFile, optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            return configuration;
        }

        public static IServiceProvider AddTestPnPCore(this IServiceCollection services)
        {
            var configuration = GetConfigurationSettings();

            services
                // Configuration
                .AddScoped<IConfiguration>(_ => configuration)
                // Logging service, get config from appsettings + add debug output handler
                .AddLogging(configure =>
                {
                    configure.AddConfiguration(configuration.GetSection("Logging"));
                    configure.AddDebug();
                })
                // Add the PnP Core SDK library services configuration from the appsettings.json file
                .Configure<PnPCoreOptions>(configuration.GetSection("PnPCore"))
                .Configure<PnPCoreAuthenticationOptions>(configuration.GetSection("PnPCore"))
                .AddPnPCoreAuthentication()
                // Add the PnP Core SDK Authentication Providers
                .AddPnPCore();

            var clientId = configuration.GetSection("PnPCore:Credentials:Configurations:CredentialManager:ClientId")?.Value;
            var tenantId = configuration.GetSection("PnPCore:Credentials:Configurations:CredentialManager:TenantId")?.Value;
            var credentialManager = configuration.GetSection("PnPCore:Credentials:Configurations:CredentialManager:CredentialManager:CredentialManagerName")?.Value;

            var resource = $"https://{new Uri(configuration["SourceTestSite"]).Authority}";

            var cmap = new CredentialManagerAuthenticationProvider(clientId, tenantId, credentialManager);
            var accessToken = cmap.GetAccessTokenAsync(new Uri(resource)).GetAwaiter().GetResult();

            services.AddTransient(p => {
                var clientContext = new ClientContext(configuration["SourceTestSite"]);
                clientContext.ExecutingWebRequest += (sender, args) =>
                {

                    if (cmap != null)
                    {
                        args.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + accessToken;
                    }
                };
                return clientContext;
            });

            return services.BuildServiceProvider();
        }

        private static string LoadTestEnvironment()
        {
            // Detect if we're running in a github workflow            
            if (RunningInGitHubWorkflow())
            {
                return "ci";
            }
            else
            {
                string testEnvironmentFile = "..\\..\\..\\env.txt";
                if (File.Exists(testEnvironmentFile))
                {
                    string content = File.ReadAllText(testEnvironmentFile);
                    if (!string.IsNullOrEmpty(content))
                    {
                        return content.Trim();
                    }
                }

                return null;
            }
        }


        internal static bool RunningInGitHubWorkflow()
        {
            var runningInCI = Environment.GetEnvironmentVariable("CI");
            if (!string.IsNullOrEmpty(runningInCI))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal const string PnPCoreSDKTestPrefix = "PNP_SDK_TEST_";
        internal static string GetPnPSdkTestAssetName(string name)
        {
            return name.StartsWith(PnPCoreSDKTestPrefix) ? name : $"{PnPCoreSDKTestPrefix}{name}";
        }
    }
}
