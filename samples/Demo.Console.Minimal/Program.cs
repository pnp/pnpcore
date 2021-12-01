using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PnP.Core.Auth;
using PnP.Core.Services;

string clientId = "c6b15c83-d569-4514-b4af-d433110123de";
string siteUrl = "https://bertonline.sharepoint.com/sites/prov-1";

// Creates and configures the host
var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) => 
    {
        // Add PnP Core SDK
        services.AddPnPCore(options =>
        {
            // Configure the interactive authentication provider as default
            options.DefaultAuthenticationProvider = new InteractiveAuthenticationProvider()
            {
                ClientId = clientId,
                RedirectUri = new Uri("http://localhost")
            };
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

