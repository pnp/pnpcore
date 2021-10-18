using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client.Extensions.Msal;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace Demo.PersistentTokenCache
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddEventSourceLogger();
                logging.AddConsole();
            })
            .ConfigureServices((hostingContext, services) =>
            {
                // Add the PnP Core SDK services
                services.AddPnPCore(options => {
                    // You can explicitly configure all the settings, or you can
                    // simply use the default values

                    options.PnPContext.GraphFirst = true;
                });
            })
            // Let the builder know we're running in a console
            .UseConsoleLifetime()
            // Add services to the container
            .Build();

            await host.StartAsync();

            using (var scope = host.Services.CreateScope())
            {
                var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();
                // Define the relevant Authentication options
                var pnpOptions = new PnPCoreAuthenticationCredentialConfigurationOptions 
                {
                    TenantId = "{TenantId}",  // Set to the target tenant id
                    ClientId = "{ClientId}",  // Set to the ClientId obtained from the Azure app
                    Interactive = new PnPCoreAuthenticationInteractiveOptions 
                    {
                        RedirectUri = new Uri("http://localhost")  // return to localhost is fine for a console app
                    }
                };

                // Replace {cache_file_name} with a meaningful name
                // Replace {cache_destination_directory} with a path to the folder you want to persist the cache
                var storageProperties = new StorageCreationPropertiesBuilder("{cache_file_name}", "{cache_destination_directory}")
                    .Build();

                var authenticationProvider = new PersistentInteractiveAuthenticationProvider(pnpOptions);
                // Register Authenticator token cache with the Msal persistent one
                await authenticationProvider.RegisterTokenStorageAsync(storageProperties);
                #region Interactive GET's
                // Replace {sharepoint_address} with your target SP url
                using (var context = await pnpContextFactory.CreateAsync(new Uri("{sharepoint_address}"), authenticationProvider))
                {
                    // ================================================================
                    // Getting data - everything is async!
                    // Same programming model, regardless of wether under the covers Microsoft Graph is used or SharePoint REST

                    // Interactive GET samples

                    // Retrieving web with lists and masterpageurl loaded ==> SharePoint REST query
                    var web = await context.Web.GetAsync(p => p.Title, p => p.Lists, p => p.MasterUrl);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===Web (REST)===");
                    Console.WriteLine($"Title: {web.Title}");
                    Console.WriteLine($"# Lists: {web.Lists.Length}");
                    Console.WriteLine($"Master page url: {web.MasterUrl}");
                    Console.ResetColor();

                    // Getting the team connected to this Modern Team site ==> Microsoft Graph query
                    var team = await context.Team.GetAsync();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===Team (Graph v1)===");
                    Console.WriteLine($"Name: {team.DisplayName}");
                    Console.WriteLine($"Visibility: {team.Visibility}");
                    Console.WriteLine($"Funsettings.AllowGiphy: {team.FunSettings.AllowGiphy}");
                    Console.ResetColor();

                }
                #endregion
            }

            host.Dispose();
        }
    }
}
