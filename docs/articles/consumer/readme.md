# Getting started with the PnP Core SDK

The PnP Core SDK is designed to be used in modern .Net development, hence it relies on dependency injection ([generic host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.1)) for it's core services. This implies that before you can actually use the PnP Core SDK you need to configure the needed services. Once that's done you can obtain a `PnPContext` from the `PnPContextFactory` and start using the library.

## Where is the code?

The PnP Core SDK is maintained in the PnP GitHub repository: https://github.com/pnp/pnpcore. You'll find:

- The code of the PnP Core SDK in the `src\sdk` folder
- Examples of how to use the PnP Core SDK in the `src\samples` folder
- The source of the documentation you are reading right now in the `docs` folder

## Referencing the PnP Core SDK in your project

At this point the PnP Core SDK has not yet been published as a nuget package, so you'll need to reference the SDK as DLL in your solution. You can build the `PnP.Core` solution and reference the DLL from the projects build output (e.g. `src\sdk\PnP.Core\bin\Debug\netstandard2.1\PnP.Core.dll`) or alternatively include the PnP Core project (`src\PnP.Core\PnP.Core.csproj`) in your project as a dependency. The latter approach does make it really easy for you to debug PnP Core when you're writing code.

## Configuring the needed services

Below snippet shows how to configure the needed services in a .Net Core console app: it's required to add and configure the `AuthenticationProviderFactory` and the `PnPContextFactory` services. Typically you would also include configuration and logging as well.

```csharp
var host = Host.CreateDefaultBuilder()
// Set environment to use
.UseEnvironment("demo") // you can eventually read it from environment variables
// Configure logging
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
            // You can optionally provide a custom ClientId, or the SDK will use a default one
            ClientId = customSettings.ClientId,
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

> [!Note]
> To learn more about how to setup authentication check the [Configuring authentication](configuring%20authentication.md) article.

## Obtaining a PnPContext

The `PnPContext` is the entry point for using the PnP Core SDK, you can create a `PnPContext` from either a SharePoint site url or the id of an Microsoft 365 group.

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

    using (var context = pnpContextFactory.Create("Microsoft 365 Group guid"))
    {
        // See next chapter on how to use the PnPContext
    }
}

// Cleanup console host
host.Dispose();
```

Next to creating a new `PnPContext` you can also clone an existing one, cloning is very convenient if you for example created a context for the root web of your site collection but now want to work with a sub site. Below snippet shows how to use cloning:

```csharp
using (var context = pnpContextFactory.Create("SiteToWorkWith"))
{
    var web = await context.Web.GetAsync();
    Console.WriteLine($"Title: {web.Title}");

    using (var subSiteContext = context.Clone(new Uri("https://contoso.sharepoint.com/sites/siteA/subsite")))
    {
        var subWeb = await subSiteContext.Web.GetAsync();
        Console.WriteLine($"Sub site title: {subWeb.Title}");
    }
}
```

## Using the PnPContext for operations on Microsoft 365

All operations on Microsoft 365 start from the `PnPContext` instance you've obtained from the `PnPContextFactory`. Below sample shows a simple get operation that requests data from Microsoft 365 and outputs it to the console:

```csharp
using (var context = pnpContextFactory.Create("SiteToWorkWith"))
{
    var web = await context.Web.GetAsync();
    Console.WriteLine($"Title: {web.Title}");
}
```

Here follows another example that shows how to define which properties need to be loaded while executing the request:

```csharp
var team = await context.Team.GetAsync(p => p.Description, p => p.FunSettings, p => p.DiscoverySettings, p => p.Members);
```

When you see an asynchronous call being used, it means that the call is executed immediately.
However, you can easily group multiple requests in a batch and send them in one call to the server via the built in batching support:

```csharp
var web = await context.Web.GetAsync();
var myList = web.Lists.GetByTitle("TestList");
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
var myList = web.Lists.GetByTitle("Documents");

if (myList != null)
{
    myList.Description = $"Updated on UTC {DateTime.UtcNow}";
    await myList.UpdateAsync();
}
```

Deleting follows a similar pattern, but now you use `DeleteAsync` or `Delete`:

```csharp
var web = await context.Web.GetAsync(p => p.Lists);
var myList = web.Lists.GetByTitle("ListToDelete");

if (myList != null)
{
    await myList.DeleteAsync();
}
```

If you like, you can also leverage a fluent syntax enriched with LINQ (Language Integrated Query). For example, in the following code excerpt you can see how to write a query for the items of a list.

```csharp
var document = context.Web.Lists.GetByTitle("Documents").Items
                            .Where(i => i.Title == "Sample Document")
                            .Load(i => i.Id, i => i.Title)
                            .FirstOrDefault();

if (document != null) 
{
    Console.WriteLine($"Document Title: {document.Title}");
}
```

Another approach to mainly limit the data that's being pulled from Microsoft 365 is using the `Include()` lambda expression, below example shows using `Include()` in a recursive manner: next to the Title property of the Web this request also loads the Lists for the Web and for each List it loads the Id, Title, DocumentTemplate and ContentTypes property. Given List ContentTypes is a collection the Name and FieldLinks properties are loaded and for FieldLinks the Name property is loaded.

```csharp
await context.Web.GetAsync(p => p.Title,
                           p => p.ContentTypes.Include(p => p.Name),
                           p => p.Lists.Include(p => p.Id, p => p.Title, p => p.DocumentTemplate,
                               p => p.ContentTypes.Include(p => p.Name,
                                    p => p.FieldLinks.Include(p => p.Name)))
                          );
```
