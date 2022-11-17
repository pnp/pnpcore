# Getting started with the PnP Core SDK

The PnP Core SDK is designed to be used in modern .Net development, hence it relies on dependency injection ([generic host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.1)) for its core services. This implies that before you can actually use the PnP Core SDK you need to configure the needed services. Once that's done you can obtain a `PnPContext` from the `PnPContextFactory` and start using the library.

## Where is the code?

The PnP Core SDK is maintained in the PnP GitHub repository: https://github.com/pnp/pnpcore. You'll find:

- The code of the PnP Core SDK in the `src\sdk` folder
- Examples of how to use the PnP Core SDK in the `samples` folder
- The source of the documentation you are reading right now in the `docs` folder

## I don't have access to a Microsoft 365 tenant

If you don't have a Microsoft 365 tenant you can, for developer purposes, always request [a free developer tenant](https://developer.microsoft.com/en-us/microsoft-365/dev-program) and use that for developing and testing your applications. When your organization already uses Microsoft 365 it's still a good practice to develop and test your applications on a non production tenant, such as the [free developer tenant](https://developer.microsoft.com/en-us/microsoft-365/dev-program).

## Learning from live code via Polyglot notebooks

If you want to read about PnP Core SDK, see the needed code and even execute the code against your tenant then checkout our Polyglot notebooks! Using Visual Studio Code you interactively use PnP Core SDK with your tenant, the available notebooks can be found [here](https://github.com/pnp/pnpcore/tree/dev/docs/polyglot#readme).

## Referencing the PnP Core SDK in your project

The recommended approach is to use the preview [PnP.Core nuget package](https://www.nuget.org/packages/PnP.Core) together with the [PnP.Core.Auth nuget package](https://www.nuget.org/packages/PnP.Core.Auth). The former is the actual PnP Core SDK library, while the latter is an helper library that provides a useful set of Authentication Providers to authenticate against Azure Active Directory.
Each night these preview packages are refreshed, so you can always upgrade to the latest dev bits by upgrading your nuget package to the latest version.

> [!Note]
>
> - If you want to use the PnP Core SDK authentication providers then simply add the [PnP.Core.Auth nuget package](https://www.nuget.org/packages/PnP.Core.Auth), the correct [PnP.Core nuget package](https://www.nuget.org/packages/PnP.Core) will be automatically added as it's a dependency of the [PnP.Core.Auth nuget package](https://www.nuget.org/packages/PnP.Core.Auth).

## Configuring the needed services

In order to configure the needed services in a .Net Core console app, you can rely on the `AddPnPCore` extension method (defined in the PnP.Core nuget package) and on the `AddPnPCoreAuthentication` method (defined in the PnP.Core.Auth nuget package), like in the following code excerpt:

```csharp
var host = Host.CreateDefaultBuilder()
// Configure logging
.ConfigureServices((hostingContext, services) =>
{
    // Add the PnP Core SDK library services
    services.AddPnPCore();
    // Add the PnP Core SDK library services configuration from the appsettings.json file
    services.Configure<PnPCoreOptions>(hostingContext.Configuration.GetSection("PnPCore"));
    // Add the PnP Core SDK Authentication Providers
    services.AddPnPCoreAuthentication();
    // Add the PnP Core SDK Authentication Providers configuration from the appsettings.json file
    services.Configure<PnPCoreAuthenticationOptions>(hostingContext.Configuration.GetSection("PnPCore"));
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
      "Timeout": "100",
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
      "DefaultConfiguration": "interactive",
      "Configurations": {
        "interactive": {
          "ClientId": "{your_client_id}",
          "TenantId": "{your_tenant_id}",
          "Interactive": {
            "RedirectUri": "http://localhost"
          }
        }
      }
    },
    "Sites": {
      "SiteToWorkWith": {
        "SiteUrl": "https://contoso.sharepoint.com/sites/pnp",
        "AuthenticationProviderName": "interactive"
      },
    }
  }
}
```

> [!Note]
> Ensure you've set "Copy to output directory" to "Copy always" for the `appsettings.json` file as otherwise the config file is not used.

You should provide the `ClientId` and `TenantId` for an application registered in Azure Active Directory and configured with proper permissions, accordingly to your needs. For example, you could register an app in Azure Active Directory with delegated permission for:

- Microsoft Graph: `Group.ReadWrite.All`
- Microsoft Graph: `User.ReadWrite.All`
- SharePoint Online: `AllSites.FullControl`
- SharePoint Online: `TermStore.ReadWrite.All`
- SharePoint Online: `User.ReadWrite.All`

As the Redirect URI, in Web platform enter __http://localhost__.

If you don't want to register a custom app in your target Azure Active Directory, you can skip the `ClientId` and `TenantId` properties and the PnP Core SDK will rely on a multi-tenant application that will be registered on your tenant, upon admin consent.

In the above example, the authentication will rely on the `InteractiveAuthenticationProvider` (defined in the PnP.Core.Auth nuget package) so that you will simply need to authenticate with a set of valid credentials for your target tenant.

If you like to configure the .Net Core console app in code, without relying on the `appsettings.json` file, you can also use the following syntax:

```csharp
var host = Host.CreateDefaultBuilder()
// Configure logging
.ConfigureServices((hostingContext, services) =>
{
  // Add the PnP Core SDK library
  services.AddPnPCore(options => {
      options.PnPContext.GraphFirst = true;
      options.HttpRequests.UserAgent = "ISV|Contoso|ProductX";

      options.Sites.Add("SiteToWorkWith", new PnPCoreSiteOptions
      {
          SiteUrl = "https://contoso.sharepoint.com/sites/pnp"
      });
  });
  services.AddPnPCoreAuthentication(
      options => {
          // Configure an Authentication Provider relying on the interactive authentication
          options.Credentials.Configurations.Add("interactive",
              new PnPCoreAuthenticationCredentialConfigurationOptions
              {
                  ClientId = "{your_client_id}",
                  TenantId = "{your_tenant_id}",
                  Interactive = new PnPCoreAuthenticationInteractiveOptions
                  {
                      RedirectUri = new Uri("http://localhost")
                  }
              });

          // Configure the default authentication provider
          options.Credentials.DefaultConfiguration = "interactive";

          // Map the site defined in AddPnPCore with the 
          // Authentication Provider configured in this action
          options.Sites.Add("SiteToWorkWith",
              new PnPCoreAuthenticationSiteOptions
              {
                  AuthenticationProviderName = "interactive"
              });
    }
  );
})
// Let the builder know we're running in a console
.UseConsoleLifetime()
// Add services to the container
.Build();
```

In advanced scenarios, you can consider using code-based configuration of registered services, like in the following code excerpt. Typically you would also include logging as well.

```csharp
var host = Host.CreateDefaultBuilder()
// Set environment to use
.UseEnvironment("demo") // you can eventually read it from environment variables
// Configure logging
.ConfigureServices((hostingContext, services) =>
{
  // Read the custom configuration from the appsettings.<environment>.json file
  var customSettings = new CustomSettings();
  hostingContext.Configuration.Bind("CustomSettings", customSettings);

  // Create an instance of the Interactive Authentication Provider
  var authenticationProvider = new InteractiveAuthenticationProvider(
                  customSettings.ClientId,
                  customSettings.TenantId,
                  customSettings.RedirectUri);

  // Add the PnP Core SDK services
  services
  .AddPnPCore(options => {

      // You can explicitly configure all the settings, or you can
      // simply use the default values
      options.PnPContext.GraphFirst = true;
      options.PnPContext.GraphCanUseBeta = true;
      options.PnPContext.GraphAlwaysUseBeta = false;

      options.HttpRequests.UserAgent = "NONISV|SharePointPnP|PnPCoreSDK";
      options.HttpRequests.MicrosoftGraph = new PnPCoreHttpRequestsGraphOptions
      {
         UseRetryAfterHeader = true,
         MaxRetries = 10,
         DelayInSeconds = 3,
         UseIncrementalDelay = true,
      };
      options.HttpRequests.SharePointRest = new PnPCoreHttpRequestsSharePointRestOptions
      {
         UseRetryAfterHeader = true,
         MaxRetries = 10,
         DelayInSeconds = 3,
         UseIncrementalDelay = true,
      };

      options.DefaultAuthenticationProvider = authenticationProvider;

      options.Sites.Add("DemoSite",
          new PnP.Core.Services.Builder.Configuration.PnPCoreSiteOptions
          {
              SiteUrl = customSettings.DemoSiteUrl,
              AuthenticationProvider = authenticationProvider
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
    "ClientId": "{client_id}",
    "TenantId": "{tenant_id}",
    "DemoSiteUrl": "https://contoso.sharepoint.com/sites/pnp",
    "RedirectUri": "http://localhost"
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

The `PnPContext` is the entry point for using the PnP Core SDK, you can create a `PnPContext` from either a SharePoint site URL or the id of a Microsoft 365 group.

> [!Note]
> You'll get a `PnPContext` for the **root web** of the site collection. Checkout the [Getting sub webs](./webs-intro.md#getting-sub-webs) content to learn how to get a `PnPContext` for the sub webs of the root web.

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

If you prefer to create a `PnPContext` by specifying the URL you need in code or you want to be able to easily switch between authentication providers then below sample shows how to so. This snippet works **without an configuration file**, all you need to do is add the needed authentication provider configuration(s) and then later on in your code acquire the needed authentication providers via the `IAuthenticationProviderFactory`. Once you have your authentication provider you can use it in the `Create` methods on the context factory:

```csharp
var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) => 
    {
        services.AddPnPCore();
        services.AddPnPCoreAuthentication(options =>
        {
            options.Credentials.Configurations.Add("interactive", 
              new PnPCoreAuthenticationCredentialConfigurationOptions
              {
                  ClientId = "c545f9ce-1c11-440b-812b-0b35217d9e83",
                  Interactive = new PnPCoreAuthenticationInteractiveOptions
                  {
                      RedirectUri = new Uri("http://localhost")
                  }
              });

            options.Credentials.Configurations.Add("usernamepassword", 
              new PnPCoreAuthenticationCredentialConfigurationOptions
              {
                  ClientId = "c545f9ce-1c11-440b-812b-0b35217d9e83",                
                  UsernamePassword = new PnPCoreAuthenticationUsernamePasswordOptions
                  {
                      Username = "joe@contoso.onmicrosoft.com",
                      Password = "xxx"
                  }
              });

        });
    })
    .UseConsoleLifetime()
    .Build();

await host.StartAsync();

using (var scope = host.Services.CreateScope())
{
    var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();
    var pnpAuthenticationProviderFactory = scope.ServiceProvider.GetRequiredService<IAuthenticationProviderFactory>();

    var interactiveAuthProvider = pnpAuthenticationProviderFactory.Create("interactive");
    var passwordManagerAuthProvider = pnpAuthenticationProviderFactory.Create("usernamepassword");

    using (var context = await pnpContextFactory.CreateAsync(new Uri("https://contoso.sharepoint.com/sites/prov-1"), 
                                                             interactiveAuthProvider))
    {
        await context.Web.LoadAsync(p => p.Title);
        Console.WriteLine($"The title of the web is {context.Web.Title}");

        using (var context2 = await pnpContextFactory.CreateAsync(new Uri("https://contoso.sharepoint.com/sites/prov-1"), 
                                                                  passwordManagerAuthProvider))
        {
            await context2.Web.LoadAsync(p => p.Title);
            Console.WriteLine($"The title of the web is {context2.Web.Title}");
        }
    }
}
```

Next to creating a new `PnPContext` you can also clone an existing one, cloning is very convenient if you for example created a context for the root web of your site collection but now want to work with a sub site. Below snippet shows how to use cloning:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    var web = await context.Web.GetAsync();
    Console.WriteLine($"Title: {web.Title}");

    using (var subSiteContext = await context.CloneAsync(new Uri("https://contoso.sharepoint.com/sites/siteA/subsite")))
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
var myList = await context.Web.Lists.GetByTitleAsync("TestList");
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
var myList = await context.Web.Lists.GetByTitleAsync("Documents");

if (myList != null)
{
    myList.Description = $"Updated on UTC {DateTime.UtcNow}";
    await myList.UpdateAsync();
}
```

Deleting follows a similar pattern, but now you use `DeleteAsync` or `Delete`:

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("ListToDelete");

if (myList != null)
{
    await myList.DeleteAsync();
}
```

If you like, you can also leverage a fluent syntax enriched with LINQ (Language Integrated Query). For example, in the following code excerpt you can see how to write a query for the items of a list.

```csharp
var list = await context.Web.Lists.GetByTitleAsync("Documents");
var document = await list.Items.Where(i => i.Title == "Sample Document")
                               .QueryProperties(i => i.Id, i => i.Title)
                               .FirstOrDefaultAsync();

if (document != null)
{
    Console.WriteLine($"Document Title: {document.Title}");
}
```

Another approach to mainly limit the data that's being pulled from Microsoft 365 is using the `QueryProperties()` method on the properties specified in the lambda expression(s), below example shows using `QueryProperties()` in a recursive manner: next to the Title property of the Web this request also loads the Lists for the Web and for each List it loads the Id, Title, DocumentTemplate and ContentTypes property. Given List ContentTypes is a collection, the Name and FieldLinks properties of each content type are loaded and, in turn, for ContentType FieldLinks, the Name property is loaded.

```csharp
await context.Web.LoadAsync(p => p.Title,
                            p => p.ContentTypes.QueryProperties(p => p.Name),
                            p => p.Lists.QueryProperties(p => p.Id,
                                                         p => p.Title,
                                                         p => p.DocumentTemplate,
                                p => p.ContentTypes.QueryProperties(p => p.Name,
                                     p => p.FieldLinks.QueryProperties(p => p.Name)))
                           );
```

The `QueryProperties()` method can also be combined with the various `GetBy` methods: below call will load the list with as title "Documents" and for that list all `ContentTypes` are loaded with all their respective `FieldLinks`.

```csharp
var web = context.Web;
var list = await web.Lists.GetByTitleAsync("Documents",
                            p => p.Title,
                            p => p.ListExperience,
                            p => p.ContentTypes.QueryProperties(p => p.Id,
                                   p => p.Name,
                                   p=>p.FieldLinks.QueryProperties(p=>p.Id, p=>p.Name)));
```
