using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Auth;
using System.Security.Cryptography.X509Certificates;

[assembly: FunctionsStartup(typeof(Demo.AzureFunction.Startup))]

namespace Demo.AzureFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = builder.GetContext().Configuration;
            var azureFunctionSettings = new AzureFunctionSettings();
            config.Bind(azureFunctionSettings);

            builder.Services.AddPnPCore(options =>
            {
                // Disable telemetry because of mixed versions on AppInsights dependencies
                options.DisableTelemetry = true;

                // Configure an authentication provider with certificate (Required for app only)
                var authProvider = new X509CertificateAuthenticationProvider(azureFunctionSettings.ClientId,
                    azureFunctionSettings.TenantId,
                    StoreName.My,
                    StoreLocation.CurrentUser,
                    azureFunctionSettings.CertificateThumbprint);
                // And set it as default
                options.DefaultAuthenticationProvider = authProvider;

                // Add a default configuration with the site configured in app settings
                options.Sites.Add("Default",
                       new PnP.Core.Services.Builder.Configuration.PnPCoreSiteOptions
                       {
                           SiteUrl = azureFunctionSettings.SiteUrl,
                           AuthenticationProvider = authProvider
                       });
            });
        }
    }
}