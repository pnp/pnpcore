# Interop with the Microsoft Graph SDK

While the focus of PnP Core SDK is mainly on SharePoint and Teams features, using a mix of Microsoft Graph, SharePoint REST and SharePoint CSOM API's, you might also want to perform additional Microsoft Graph calls. A convenient way to do so is by using the [Microsoft Graph SDK](https://www.nuget.org/packages/Microsoft.Graph) as explained in this article.

## Using the Microsoft Graph SDK when the PnP Core SDK was already configured

In PnP Core SDK a PnPContext is used while in the Microsoft Graph SDK a GraphServiceClient is used. Below sample shows how create a GraphServiceClient for a given PnPContext.

```csharp
using (var pnpCoreContext = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Use PnP Core SDK (Microsoft Graph / SPO Rest) to load the web title
    var web = pnpCoreContext.Web.Get(p => p.Title);

    // Create a Graph Service client and perform a Graph call using the Microsoft Graph .NET SDK
    var graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
    {
        return pnpCoreContext.AuthenticationProvider.AuthenticateRequestAsync(new Uri("https://graph.microsoft.com"), requestMessage);
    }));

    var me = await graphServiceClient.Me.Request().GetAsync();
}
```

## Using the PnP Core SDK when the Microsoft Graph SDK was already configured

When you configure the Microsoft Graph SDK using MSAL you typically have code that gets an access token. This access token is then used via the `DelegateAuthenticationProvider` Microsoft Graph authentication provider, but the same approach can be taken using the PnP Core SDK `ExternalAuthenticationProvider`. Below console application shows Microsoft Graph and PnP Core SDK sharing the same code to get access tokens:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using PnP.Core.Auth;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HelloGraph
{
    class Program
    {
        private static readonly string[] scopes = new string[] { "user.read", "sites.fullcontrol.all" };
        private const string clientId = "<Azure AD application id>";
        private const string tenant = "organizations"; // Alternatively tenant name, e.g. contoso.onmicrosoft.com"
        private const string authority = "https://login.microsoftonline.com/" + tenant;
        private static IPublicClientApplication publicClientApp;
        private static string msGraphURL = "https://graph.microsoft.com/v1.0/";
        private static AuthenticationResult authResult;

        public static async Task Main(string[] args)
        {

            var host = Host.CreateDefaultBuilder()
            // Configure PnP services
            .ConfigureServices((hostingContext, services) =>
            {
                services.AddPnPCore();
            })

            // Let the builder know we're running in a console
            .UseConsoleLifetime()
            // Add services to the container
            .Build();

            // Build the host
            await host.StartAsync();

            // Sign-in user using MSAL and obtain an access token for MS Graph
            GraphServiceClient graphClient = new GraphServiceClient(
                msGraphURL,
                new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", await SignInUserAndGetTokenUsingMSAL(scopes));
                }));

            // Call the /me endpoint of Graph
            User graphUser = await graphClient.Me.Request().GetAsync();
            Console.WriteLine($"User UPN: {graphUser.UserPrincipalName}");

            // Create a PnP Core SDK context
            var pnpContextFactory = host.Services.GetRequiredService<IPnPContextFactory>();

            using (var pnpContext = await pnpContextFactory.CreateAsync(
                        new Uri("https://bertonline.sharepoint.com/sites/prov-1"),
                        new ExternalAuthenticationProvider((resourceUri, scopes) =>
                        {
                            return SignInUserAndGetTokenUsingMSAL(scopes).GetAwaiter().GetResult();
                        }))
                )
            {
                var web = await pnpContext.Web.GetAsync(p => p.Title);
                Console.WriteLine($"Web title: {web.Title}");
            }

            host.Dispose();
        }

        private static async Task<string> SignInUserAndGetTokenUsingMSAL(string[] scopes)
        {
            if (publicClientApp == null)
            {
                // Initialize the MSAL library by building a public client application
                publicClientApp = PublicClientApplicationBuilder.Create(clientId)
                    .WithAuthority(authority)
                    .WithRedirectUri("http://localhost")
                    .Build();
            }

            // It's good practice to not do work on the UI thread, so use ConfigureAwait(false) whenever possible.
            IEnumerable<IAccount> accounts = await publicClientApp.GetAccountsAsync().ConfigureAwait(false);
            try
            {
                authResult = await publicClientApp.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                                                  .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilentAsync. 
                // This indicates you need to call AcquireTokenAsync to acquire a token
                authResult = await publicClientApp.AcquireTokenInteractive(scopes)
                                                  .ExecuteAsync()
                                                  .ConfigureAwait(false);
            }

            return authResult.AccessToken;
        }

    }
}
```
