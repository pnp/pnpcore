using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Auth;
using System.Security.Cryptography.X509Certificates;
using PnP.Core.Services.Builder.Configuration;
using System;
[assembly: FunctionsStartup(typeof(Demo.AzFunction.ManagedIdentity.Startup))]


namespace Demo.AzFunction.ManagedIdentity
{
    // https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var appConfig = new AppConfig();

            // Add the PnP Core SDK services
            builder.Services.AddPnPCore(options =>
            {
                // Disable telemetry because of mixed versions on AppInsights dependencies
                options.DisableTelemetry = true;

                //If Managed Identity is configured
                if (appConfig.isMSI)
                {
                    var authProvider = new ManagedIdentityTokenProvider();

                    // And set it as default
                    options.DefaultAuthenticationProvider = authProvider;

                    // Add a default configuration with the site configured in app settings
                    options.Sites.Add("Default",
                        new PnPCoreSiteOptions
                        {
                            SiteUrl = appConfig.SiteUrl,
                            AuthenticationProvider = authProvider
                        });
                }
                else
                {
                    Console.WriteLine("Local DEV using cert auth");

                    var appConfigCert = new AppConfigCert();
                    // Configure an authentication provider with certificate (Required for app only)
                    // App-only authentication against SharePoint Online requires certificate based authentication for calling the "classic" SharePoint REST/CSOM APIs. The SharePoint Graph calls can work with clientid+secret, but since PnP Core SDK requires both type of APIs (as not all features are exposed via the Graph APIs) you need to use certificate based auth.
                    var authProvider = new X509CertificateAuthenticationProvider(appConfigCert.ClientId,
                        appConfigCert.TenantId,
                        StoreName.My,
                        StoreLocation.CurrentUser,
                        appConfigCert.CertificateThumbprint);
                    // And set it as default
                    options.DefaultAuthenticationProvider = authProvider;

                    // Add a default configuration with the site configured in app settings
                    options.Sites.Add("Default",
                           new PnP.Core.Services.Builder.Configuration.PnPCoreSiteOptions
                           {
                               SiteUrl = appConfig.SiteUrl,
                               AuthenticationProvider = authProvider
                           });
                }
            });


        }
    }
}