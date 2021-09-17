using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using PnP.Core.Auth;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services;
using PnP.Core.Services.Builder.Configuration;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.SharePoint;

namespace PnP.Core.Transformation.ConsoleSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region Dependency Injection plumbing

            var host = Host.CreateDefaultBuilder()
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddEventSourceLogger();
                logging.AddConsole();
            })
            .ConfigureServices((hostingContext, services) =>
            {
                // Register the PnP Core services
                services.AddPnPCore();
                services.Configure<PnPCoreOptions>(hostingContext.Configuration.GetSection("PnPCore"));

                // Register the PnP Core authentication services
                services.AddPnPCoreAuthentication();
                services.Configure<PnPCoreAuthenticationOptions>(hostingContext.Configuration.GetSection("PnPCore"));

                // Register the PnP Transformation Framework services with SharePoint as a data source
                services.AddPnPSharePointTransformation(null, spOptions =>
                {
                    //spOptions.WebPartMappingFile = @"C:\github\pnpcore\src\sdk\PnP.Core.Transformation.SharePoint\MappingFiles\webpartmapping.xml";
                    //spOptions.PageLayoutMappingFile = @"C:\github\pnpcore\src\sdk\PnP.Core.Transformation.SharePoint\MappingFiles\pagelayoutmapping.xml";
                    spOptions.CopyPageMetadata = true;
                    spOptions.KeepPageSpecificPermissions = true;
                    spOptions.RemoveEmptySectionsAndColumns = true;
                    spOptions.ShouldMapUsers = true;
                    spOptions.TargetPageTakesSourcePageName = true;
                });

                // Register the CSOM ClientContext for the data source
                services.AddTransient(p => {
                    var clientContext = new ClientContext(hostingContext.Configuration["SourceSite"]);
                    clientContext.ExecutingWebRequest += (sender, args) =>
                    {
                        var resource = $"https://{new Uri(hostingContext.Configuration["SourceSite"]).Authority}";

                        var clientId = hostingContext.Configuration.GetSection("PnPCore:Credentials:Configurations:CredentialManager:ClientId")?.Value;
                        var tenantId = hostingContext.Configuration.GetSection("PnPCore:Credentials:Configurations:CredentialManager:TenantId")?.Value;
                        var credentialManager = hostingContext.Configuration.GetSection("PnPCore:Credentials:Configurations:CredentialManager:CredentialManager:CredentialManagerName")?.Value;

                        var cmap = new CredentialManagerAuthenticationProvider(clientId, tenantId, credentialManager);
                        if (cmap != null)
                        {
                            args.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + cmap.GetAccessTokenAsync(new Uri(resource)).GetAwaiter().GetResult();
                        }
                    };
                    return clientContext;
                });

            })
            // Let the builder know we're running in a console
            .UseConsoleLifetime()
            // Add services to the container
            .Build();

            await host.StartAsync();

            #endregion

            using (var scope = host.Services.CreateScope())
            {
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();
                var pageTransformator = scope.ServiceProvider.GetRequiredService<IPageTransformator>();
                var sourceContext = scope.ServiceProvider.GetRequiredService<ClientContext>();

                using (var targetContext = await pnpContextFactory.CreateAsync("TargetSite"))
                {
                    var sourcePageUri = configuration.GetValue<string>("SourcePageUri");
                    var sourceUri = new Uri(sourcePageUri);
                    var result = await pageTransformator.TransformSharePointAsync(sourceContext, targetContext, sourceUri);

                    Console.WriteLine($"Here is the URL of the transformed page: {result}");
                }
            }
        }
    }
}
