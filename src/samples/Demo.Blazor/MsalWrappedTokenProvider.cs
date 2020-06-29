using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace Demo.Blazor
{
    public class MsalWrappedTokenProvider : IOAuthAccessTokenProvider
    {
        private readonly IAccessTokenProvider _accessTokenProvider;

        public MsalWrappedTokenProvider(IAccessTokenProvider accessTokenProvider)
        {
            _accessTokenProvider = accessTokenProvider;
        }

        private const string MicrosoftGraphScope = "Sites.FullControl.All";
        private const string SharePointOnlineScope = "AllSites.FullControl";

        private async Task<string> GetAccessTokenAsync(string[] scopes)
        {
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

        public async Task<string> GetAccessTokenAsync(Uri resourceUri)
        {
            return await GetAccessTokenAsync(GetRelevantScopes(resourceUri));
        }
    }
}
