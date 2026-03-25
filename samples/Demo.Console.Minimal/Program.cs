using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services;

string clientId = "<YourClientId>";
string tenantId = "<YourTenantId>";
string siteUrl = "<YourSiteUrl>";

// Creates and configures the host
var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) => 
    {
        // Add PnP Core SDK
        services.AddPnPCore();
        
        // Add PnP Core SDK Authentication
        services.AddPnPCoreAuthentication(options =>
        {
            // Configure interactive authentication
            options.Credentials.Configurations.Add("interactive",
                new PnPCoreAuthenticationCredentialConfigurationOptions
                {
                    ClientId = clientId,
                    TenantId = tenantId,
                    Interactive = new PnPCoreAuthenticationInteractiveOptions
                    {
                        RedirectUri = new Uri("http://localhost")
                    }
                });

            // Set as default configuration
            options.Credentials.DefaultConfiguration = "interactive";
        });
    })
    .UseConsoleLifetime()
    .Build();

// Start the host
await host.StartAsync();

using (var scope = host.Services.CreateScope())
{
    // Ask an IPnPContextFactory from the host
    var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();

    // Create a PnPContext
    using (var context = await pnpContextFactory.CreateAsync(new Uri(siteUrl)))
    {
        // Load the Title property of the site's root web
        await context.Web.LoadAsync(p => p.Title);
        Console.WriteLine($"The title of the web is {context.Web.Title}");
    }
}

