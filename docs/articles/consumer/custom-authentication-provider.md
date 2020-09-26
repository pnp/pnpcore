# Creating a custom Authentication Provider for PnP.Core

There might be implementations where the default provided authentication providers do not offer the needed features (e.g. when authenticating Blazor WebAssembly applications). If that's the case, you always have the option to write your own authentication provider and use that when working with the PnP Core SDK.

## Creating a custom authentication provider

When using Blazor WebAssembly applications we want to ask the Blazor framework to deliver us an access token for the logged on user and then use that access token in the PnP Core SDK. There's no default authentication provider in PnP.Core.Auth for that, so let's roll our own. Writing an authentication provider comes down to implementing the `IAuthenticationProvider` interface defined in the `PnP.Core.Services` namespace (in the `PnP.Core` assembly). This interface requires 3 methods to be implemented:

- `Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request)` : authenticates a web requests by adding an access token to it
- `Task<string> GetAccessTokenAsync(Uri resource, String[] scopes)` : gets an access token for the requested resource and scope
- `Task<string> GetAccessTokenAsync(Uri resource)` : gets an access token for the requested resource and its default scopes

Below sample shows a custom authentication provider that can be used in Blazor WebAssembly applications:

```csharp
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using PnP.Core.Services;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Demo.Blazor
{
    /// <summary>
    /// Custom authentication provider that uses the WebAssembly access token provider to obtain an access token
    /// </summary>
    public class MsalWrappedTokenProvider : IAuthenticationProvider
    {
        private readonly IAccessTokenProvider _accessTokenProvider;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="accessTokenProvider">WebAssembly access token provider instance</param>
        public MsalWrappedTokenProvider(IAccessTokenProvider accessTokenProvider)
        {
            _accessTokenProvider = accessTokenProvider;
        }

        private const string MicrosoftGraphScope = "Sites.FullControl.All";
        private const string SharePointOnlineScope = "AllSites.FullControl";

        private string[] GetRelevantScopes(Uri resourceUri)
        {
            if (resourceUri.ToString() == "https://graph.microsoft.com")
            {
                return new[] { $"{resourceUri}/{MicrosoftGraphScope}" };
            }
            else
            {
                string resource = $"{resourceUri.Scheme}://{resourceUri.DnsSafeHost}";
                return new[] { $"{resource}/{SharePointOnlineScope}" };
            }
        }

        /// <summary>
        /// Authenticate the web request
        /// </summary>
        /// <param name="resource">Resource to get an access token for</param>
        /// <param name="request">Request to add the access token on</param>
        /// <returns></returns>
        public async Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("bearer",
                await GetAccessTokenAsync(resource).ConfigureAwait(false));
        }

        /// <summary>
        /// Gets an access token for the requested resource and scopes
        /// </summary>
        /// <param name="resource">Resource to get access token for</param>
        /// <param name="scopes">Scopes to use when getting the access token</param>
        /// <returns>Obtained access token</returns>
        public async Task<string> GetAccessTokenAsync(Uri resource, string[] scopes)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            var tokenResult = await _accessTokenProvider.RequestAccessToken(new AccessTokenRequestOptions()
            {
                // The scopes must specify the needed permissions for the app to work
                Scopes = scopes,
            }).ConfigureAwait(false);

            if (!tokenResult.TryGetToken(out AccessToken accessToken))
            {
                throw new Exception("An error occured while trying to acquire the access token...");
            }

            return accessToken.Value;
        }

        /// <summary>
        /// Gets an access token for the requested resource
        /// </summary>
        /// <param name="resource">Resource to get access token for</param>
        /// <returns>Obtained access token</returns>
        public async Task<string> GetAccessTokenAsync(Uri resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            return await GetAccessTokenAsync(resource, GetRelevantScopes(resource));
        }
    }
}
```

## Using a custom authentication provider

Once you've added the custom authentication provider to your project, you need to configure it in the pipeline of PnP Core SDK. This is done by passing an authentication provider instance to the `CreateAsync` or `Create` [methods of the `IPnPContextFactory` you're using](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContextFactory.html#methods).

```csharp
using(var context = await pnpContextFactory.CreateAsync(new Uri("https://contoso.sharepoint.com/sites/siteA"), myCustomAuthProvider))
{
    // use context to read/update Microsoft 365 data
}
```
