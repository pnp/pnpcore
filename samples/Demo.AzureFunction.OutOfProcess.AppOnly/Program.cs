using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PnP.Core.Auth;
using System.Security.Cryptography.X509Certificates;

namespace ProvisioningDemo
{
    public class Program
    {
        public static void Main()
        {
            AzureFunctionSettings azureFunctionSettings = null;

            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices((context, services) =>
                {
                    // Add our global configuration instance
                    services.AddSingleton(options =>
                    {
                        var configuration = context.Configuration;
                        azureFunctionSettings = new AzureFunctionSettings();
                        configuration.Bind(azureFunctionSettings);
                        return configuration;
                    });

                    // Add and configure PnP Core SDK
                    services.AddPnPCore(options =>
                    {
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

                })
                .Build();

            host.Run();
        }
    }
}