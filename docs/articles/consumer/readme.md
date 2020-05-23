# Getting started with the PnP Core SDK

The PnP Core SDK is designed to be used in modern .Net development, hence it relies on dependency injection ([generic host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.1)) for it's core services. This implies that before you can actually use the PnP Core SDK you need to configure the needed services. Once that's done you can obtain a `PnPContext` from the `PnPContextFactory`and start using the library.

## Configuring the needed services

Below snippet shows how to configure the needed services in a .Net Core console app: it's required to add and configure the `AuthenticationProviderFactory` and `PnPContextFactory` services. Typically you would also include configuration and logging as well.

```csharp
var host = Host.CreateDefaultBuilder()
// Set environment to use
.UseEnvironment("demo")
// Configure logging
.ConfigureLogging((hostingContext, logging) =>
{
    logging.AddEventSourceLogger();
    logging.AddConsole();
})
.ConfigureServices((hostingContext, services) =>
{
    var customSettings = new CustomSettings();
    hostingContext.Configuration.Bind("CustomSettings", customSettings);

    services
    // Setup PnP authentication providers
    .AddAuthenticationProviderFactory(options =>
    {
        // CredentialManager provider
        options.Configurations.Add(new OAuthCredentialManagerConfiguration
        {
            Name = "CredentialManagerAuthentication",
            CredentialManagerName = customSettings.CredentialManager,
            // You can optionally provide a custom ClientId, or the SDK will use a default one 
            ClientId = customSettings.ClientId, 
        });
        // Username + Pwd provider
        options.Configurations.Add(new OAuthUsernamePasswordConfiguration
        {
            Name = "UsernameAndPasswordAuthentication",
            Username = customSettings.UserPrincipalName,
            Password = customSettings.Password.ToSecureString(),
        });
        // Set default provider
        options.DefaultConfiguration = "CredentialManagerAuthentication";
    })
    // Setup the PnP context factory
    .AddPnPContextFactory(options =>
    {
        options.Configurations.Add(new PnPContextFactoryOptionsConfiguration
        {
            Name = "SiteToWorkWith",
            SiteUrl = new Uri(customSettings.TargetSiteUrl),
            AuthenticationProviderName = "CredentialManagerAuthentication",
        });
    });
})
// Let the builder know we're running in a console
.UseConsoleLifetime()
// Add services to the container
.Build();
```

In above sample the following configuration file is used: `appsettings.demo.json`

```json
{
  "CustomSettings": {
    "TargetSiteUrl": "https://contoso.sharepoint.com/sites/pnp",
    "CredentialManager": "mycreds",
    "UserPrincipalName": "joe@contoso.onmicrosoft.com",
    "Password": "password",
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## Obtaining a PnPContext

```csharp
// Start console host
await host.StartAsync();

using (var scope = host.Services.CreateScope())
{
    // Obtain a PnP Context factory
    var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();
    // Use the PnP Context factory to get a PnPContext for the given configuration
    using (var context = pnpContextFactory.Create("SiteToWorkWith"))
    {
        // See next chapter on how to use the PnPContext
    }
}

// Cleanup console host
host.Dispose();
```

## Using the PnPContext for operations on Microsoft 365

All operations on Microsoft 365 start from the `PnPContext` instance you've obtained from the `PnPContextFactory`. Below sample shows a simple get operation requests data from Microsoft 365 and outputs it to the console:

```csharp
using (var context = pnpContextFactory.Create("SiteToWorkWith"))
{
    var web = await context.Web.GetAsync();
    Console.WriteLine($"Title: {web.Title}");
}
```

An other example shows how to define which properties need to be loaded:

```csharp
var team = await context.Team.GetAsync(p => p.Description, p => p.FunSettings, p => p.DiscoverySettings, p => p.Members);
```

When you see an asynchronous call being used it means that the call is executed immediately, but you can just as easy group multiple requests in a batch and send them in one call to the server via the built in batching support:

```csharp
var web = await context.Web.GetAsync();
var myList = web.Lists.Where(p => p.Title.Equals("TestList")).FirstOrDefault();
if (myList != null)
{
    // Create three list items and add them via single server request
    Dictionary<string, object> values = new Dictionary<string, object>
    {
        { "Title", "PnP Rocks!" }
    };

    myList.Items.Add(values);
    myList.Items.Add(values);
    myList.Items.Add(values);
    // Send batch to the server
    await context.ExecuteAsync();
}
```

To update Microsoft 365 you simply update the needed properties in your model and then call `UpdateAsync` or `Update` (used for batching):

```csharp
var web = await context.Web.GetAsync(p => p.Lists);
var myList = web.Lists.Where(p => p.Title.Equals("Documents")).FirstOrDefault();

if (myList != null)
{
    myList.Description = $"Updated on UTC {DateTime.UtcNow}";
    await myList.UpdateAsync();
}
```

Deleting follows a similar patters, but now you use `DeleteAsync` or `Delete`:

```csharp
var web = await context.Web.GetAsync(p => p.Lists);
var myList = web.Lists.Where(p => p.Title.Equals("ListToDelete")).FirstOrDefault();

if (myList != null)
{
    await myList.DeleteAsync();
}
```
