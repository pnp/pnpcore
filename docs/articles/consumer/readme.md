# Getting started with the PnP Core SDK

The PnP Core SDK is designed to be used in modern .Net development, hence it relies on dependency injection ([generic host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.1)) for its core services. This implies that before you can actually use the PnP Core SDK you need to configure the needed services. Once that's done you can obtain a `PnPContext` from the `PnPContextFactory` and start using the library.

## Where is the code?

The PnP Core SDK is maintained in the PnP GitHub repository: https://github.com/pnp/pnpcore. You'll find:

- The code of the PnP Core SDK in the `src\sdk` folder
- Examples of how to use the PnP Core SDK in the `src\samples` folder
- The source of the documentation you are reading right now in the `docs` folder

## Referencing the PnP Core SDK in your project

The recommended approach is to use the preview [PnP.Core nuget package](https://www.nuget.org/packages/PnP.Core). Each night this preview package is refreshed so you can always upgrade to the latest dev bits by upgrading your nuget package to the latest version. 

> [!Note]
> There are 2 package flavors: a `-preview` version and a `-blazor-preview` version. The latter one is meant to be used in Blazor Web Assembly projects and will exist until one of the SDK's references gets supported on Blazor.

If you want to debug the SDK code you can include the PnP Core project (`src\PnP.Core\PnP.Core.csproj`) in your project as a dependency.

## Configuring the needed services

In order to configure the needed services in a .Net Core console app, you can rely on the `AddPnPCore` extension method, like in the following code excerpt:

```csharp
var host = Host.CreateDefaultBuilder()
// Set environment to use
.UseEnvironment("demo") // you can eventually read it from environment variables
// Configure logging
// Configure logging
.ConfigureServices((hostingContext, services) =>
{
    // Add the PnP Core SDK library services
    services.AddPnPCore();
    // Add the PnP Core SDK library services configuration from the appsettings.json file
    services.Configure<PnPCoreOptions>(Configuration.GetSection("PnPCore"));
})
// Let the builder know we're running in a console
.UseConsoleLifetime()
// Add services to the container
.Build();
```

And you will also need to provide the configuration in the `appsettings.json` file, using a configuration section like the following one:

```json
{
  "PnPCore": {
    "DisableTelemetry": "false",
    "HttpRequests": {
      "UserAgent": "ISV|Contoso|ProductX",
      "SharePointRest": {
        "UseRetryAfterHeader": "false",
        "MaxRetries": "10",
        "DelayInSeconds": "3",
        "UseIncrementalDelay": "true"
      },
      "MicrosoftGraph": {
        "UseRetryAfterHeader": "true",
        "MaxRetries": "10",
        "DelayInSeconds": "3",
        "UseIncrementalDelay": "true"
      }
    },
    "PnPContext": {
      "GraphFirst": "true",
      "GraphCanUseBeta": "true",
      "GraphAlwaysUseBeta": "false"
    },
    "Credentials": {
      "CredentialManagerAuthentication": {
        "CredentialManagerName": "mycreds"
      }
    },
    "Sites": {
      "SiteToWorkWith": {
        "SiteUrl": "https://contoso.sharepoint.com/sites/pnp",
        "AuthenticationProviderName": "CredentialManagerAuthentication"
      },
    }
  }
}
```

If you like to configure the .Net Core console app in code, without relying on the `appsettings.json` file, you can also use the following syntax:

```csharp
var host = Host.CreateDefaultBuilder()
// Set environment to use
.UseEnvironment("demo") // you can eventually read it from environment variables
// Configure logging
// Configure logging
.ConfigureServices((hostingContext, services) =>
{
    // Add the PnP Core SDK library services with code based settings
    services.AddPnPCore(options => {
        options.PnPContext.GraphFirst = true;
        options.HttpRequests.UserAgent = "ISV|Contoso|ProductX";

        options.Sites.Add("SiteToWorkWith", new PnPCoreSiteOptions
        {
            SiteUrl = "https://contoso.sharepoint.com/sites/pnp",
            AuthenticationProviderName = "CredentialManagerAuthentication"
        });

        options.Credentials.Add("CredentialManagerAuthentication", new PnPCoreCredentialOptions
        {
            CredentialManagerName = "mycreds"
        });
    });
})
// Let the builder know we're running in a console
.UseConsoleLifetime()
// Add services to the container
.Build();
```

In advanced scenarios, you can consider using low level services registration, like in the following code excerpt. Typically you would also include configuration and logging as well.

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

The `PnPContext` is the entry point for using the PnP Core SDK, you can create a `PnPContext` from either a SharePoint site url or the id of a Microsoft 365 group.

```csharp
// Start console host
await host.StartAsync();

using (var scope = host.Services.CreateScope())
{
    // Obtain a PnP Context factory
    var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();
    // Use the PnP Context factory to get a PnPContext for the given configuration
    using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
    {
        // See next chapter on how to use the PnPContext
    }

    using (var context = await pnpContextFactory.CreateAsync("Microsoft 365 Group guid"))
    {
        // See next chapter on how to use the PnPContext
    }
}

// Cleanup console host
host.Dispose();
```

Next to creating a new `PnPContext` you can also clone an existing one, cloning is very convenient if you for example created a context for the root web of your site collection but now want to work with a sub site. Below snippet shows how to use cloning:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
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
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
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
var myList = await web.Lists.GetByTitleAsync("TestList");
if (myList != null)
{
    // Create three list items and add them via single server request
    Dictionary<string, object> values = new Dictionary<string, object>
    {
        { "Title", "PnP Rocks!" }
    };

    await myList.Items.AddBatchAsync(values);
    await myList.Items.AddBatchAsync(values);
    await myList.Items.AddBatchAsync(values);

    // Send batch to the server
    await context.ExecuteAsync();
}
```

To update Microsoft 365 you simply update the needed properties in your model and then call `UpdateAsync` or `UpdateBatchAsync` (used for batching):

```csharp
var web = await context.Web.GetAsync(p => p.Lists);
var myList = await web.Lists.GetByTitleAsync("Documents");

if (myList != null)
{
    myList.Description = $"Updated on UTC {DateTime.UtcNow}";
    await myList.UpdateAsync();
}
```

Deleting follows a similar pattern, but now you use `DeleteAsync` or `Delete`:

```csharp
var web = await context.Web.GetAsync(p => p.Lists);
var myList = await web.Lists.GetByTitleAsync("ListToDelete");

if (myList != null)
{
    await myList.DeleteAsync();
}
```

If you like, you can also leverage a fluent syntax enriched with LINQ (Language Integrated Query). For example, in the following code excerpt you can see how to write a query for the items of a list.

```csharp
var document = (await context.Web.Lists.GetByTitleAsync("Documents")).Items
                            .Where(i => i.Title == "Sample Document")
                            .Load(i => i.Id, i => i.Title)
                            .FirstOrDefault();

if (document != null)
{
    Console.WriteLine($"Document Title: {document.Title}");
}
```

Another approach to mainly limit the data that's being pulled from Microsoft 365 is using the `LoadProperties()` method on the properties specified in the lambda expression(s), below example shows using `LoadProperties()` in a recursive manner: next to the Title property of the Web this request also loads the Lists for the Web and for each List it loads the Id, Title, DocumentTemplate and ContentTypes property. Given List ContentTypes is a collection, the Name and FieldLinks properties of each content type are loaded and, in turn, for ContentType FieldLinks, the Name property is loaded.

```csharp
await context.Web.GetAsync(p => p.Title,
                           p => p.ContentTypes.LoadProperties(p => p.Name),
                           p => p.Lists.LoadProperties(p => p.Id, p => p.Title, p => p.DocumentTemplate,
                               p => p.ContentTypes.LoadProperties(p => p.Name,
                                    p => p.FieldLinks.LoadProperties(p => p.Name)))
                          );
```
