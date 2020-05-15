using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    public class OAuthAuthenticationProvider : IAuthenticationProvider
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string tokenEndpoint = "https://login.microsoftonline.com/common/oauth2/token";

        // Microsoft SharePoint Online Management Shell client id
        //private static readonly string aadAppId = "9bc3ab49-b65d-410a-85ad-de819febfddc";
        // PnP Office 365 Management Shell 
        private const string aadAppId = "31359c7f-bd7e-475c-86db-fdb8c937548e";

        private readonly ILogger log;
        private IAuthenticationProviderConfiguration configuration;

        // SharePoint token cache handling
        private static readonly SemaphoreSlim semaphoreSlimSharePoint = new SemaphoreSlim(1);
        private ConcurrentDictionary<string, string> sharePointTokenCache = new ConcurrentDictionary<string, string>();

        private static readonly SemaphoreSlim semaphoreSlimMicrosoftGraph = new SemaphoreSlim(1);
        private ConcurrentDictionary<string, string> microsoftGraphTokenCache = new ConcurrentDictionary<string, string>();

        public OAuthAuthenticationProvider(
            ILogger<OAuthAuthenticationProvider> logger)
        {
            log = logger;
        }

        public void Configure(IAuthenticationProviderConfiguration configuration)
        {
            this.configuration = configuration;
        }

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

            var (Username, Password) = GetCredential();
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", await EnsureSharePointAccessTokenAsync(resource, Username, Password).ConfigureAwait(false));
        }

        private async Task<string> GetMicrosoftGraphAccessTokenAsync()
        {
            var (Username, Password) = GetCredential();
            return await EnsureMicrosoftGraphAccessTokenAsync(Username, Password).ConfigureAwait(false);
        }

        private async Task<string> GetSharePointOnlineAccessTokenAsync(Uri resource)
        {
            var (Username, Password) = GetCredential();
            return await EnsureSharePointAccessTokenAsync(resource, Username, Password).ConfigureAwait(false);
        }

        public async Task<string> GetAccessTokenAsync(Uri resource, string[] scopes)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (resource.AbsoluteUri.Equals(PnPConstants.MicrosoftGraphBaseUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                return await GetMicrosoftGraphAccessTokenAsync().ConfigureAwait(false);
            }
            else if (resource.AbsoluteUri.ToLower().Contains("sharepoint.com"))
            {
                return await GetSharePointOnlineAccessTokenAsync(resource).ConfigureAwait(false);
            }
            else
            {
                return default(string);
            }
        }

        private static async Task<string> AcquireTokenAsync(Uri resourceUri, string username, string password)
        {
            string resource = $"{resourceUri.Scheme}://{resourceUri.DnsSafeHost}";

            var body = $"resource={resource}&client_id={aadAppId}&grant_type=password&username={username}&password={password}";
            using (var stringContent = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"))
            {

                var result = await httpClient.PostAsync(tokenEndpoint, stringContent).ContinueWith((response) =>
                {
                    return response.Result.Content.ReadAsStringAsync().Result;
                }).ConfigureAwait(false);

                JObject jobject = JObject.Parse(result);
                var token = jobject["access_token"].Value<string>();
                return token;
            }
        }

        private async Task<string> EnsureSharePointAccessTokenAsync(Uri resourceUri, string userPrincipalName, string userPassword)
        {
            string accessTokenFromCache = TokenFromCache(resourceUri, sharePointTokenCache);
            if (accessTokenFromCache == null)
            {
                await semaphoreSlimSharePoint.WaitAsync().ConfigureAwait(false);
                try
                {
                    // No async methods are allowed in a lock section
                    string accessToken = await AcquireTokenAsync(resourceUri, userPrincipalName, userPassword).ConfigureAwait(false);
                    log.LogInformation($"Successfully requested new access token resource {resourceUri.DnsSafeHost} and user {userPrincipalName}");
                    AddTokenToCache(resourceUri, sharePointTokenCache, accessToken);

                    // Spin up a thread to invalidate the access token once's it's expired
                    ThreadPool.QueueUserWorkItem(async (obj) =>
                    {
                        try
                        {
                            // Wait until we're 5 minutes before the planned token expiration
                            Thread.Sleep(CalculateThreadSleep(accessToken));
                            // Take a lock to ensure no other threads are updating the SharePoint Access token at this time
                            await semaphoreSlimSharePoint.WaitAsync().ConfigureAwait(false);
                            RemoveTokenFromCache(resourceUri, sharePointTokenCache);
                            log.LogInformation($"Cached token for resource {resourceUri.DnsSafeHost} and user {userPrincipalName} expired");
                        }
                        catch (Exception ex)
                        {
                            log.LogError(ex, $"Something went wrong during cache token invalidation: {ex.Message}");
                            RemoveTokenFromCache(resourceUri, sharePointTokenCache);
                        }
                        finally
                        {
                            semaphoreSlimSharePoint.Release();
                        }
                    });

                    return accessToken;

                }
                finally
                {
                    semaphoreSlimSharePoint.Release();
                }
            }
            else
            {
                log.LogInformation($"Returning token from cache for resource {resourceUri.DnsSafeHost} and user {userPrincipalName}");
                return accessTokenFromCache;
            }
        }

        private async Task<string> EnsureMicrosoftGraphAccessTokenAsync(string userPrincipalName, string userPassword)
        {
            Uri resourceUri = PnPConstants.MicrosoftGraphBaseUri;
            string accessTokenFromCache = TokenFromCache(resourceUri, microsoftGraphTokenCache);
            if (accessTokenFromCache == null)
            {
                await semaphoreSlimMicrosoftGraph.WaitAsync().ConfigureAwait(false);
                try
                {
                    // No async methods are allowed in a lock section
                    string accessToken = await AcquireTokenAsync(resourceUri, userPrincipalName, userPassword).ConfigureAwait(false);
                    log.LogInformation($"Successfully requested new access token resource {resourceUri.DnsSafeHost} and user {userPrincipalName}");
                    AddTokenToCache(resourceUri, microsoftGraphTokenCache, accessToken);

                    // Spin up a thread to invalidate the access token once's it's expired
                    ThreadPool.QueueUserWorkItem(async (obj) =>
                    {
                        try
                        {
                            // Wait until we're 5 minutes before the planned token expiration
                            Thread.Sleep(CalculateThreadSleep(accessToken));
                            // Take a lock to ensure no other threads are updating the SharePoint Access token at this time
                            await semaphoreSlimMicrosoftGraph.WaitAsync().ConfigureAwait(false);
                            RemoveTokenFromCache(resourceUri, microsoftGraphTokenCache);
                            log.LogInformation($"Cached token for resource {resourceUri.DnsSafeHost} and user {userPrincipalName} expired");
                        }
                        catch (Exception ex)
                        {
                            log.LogError(ex, $"Something went wrong during cache token invalidation: {ex.Message}");
                            RemoveTokenFromCache(resourceUri, microsoftGraphTokenCache);
                        }
                        finally
                        {
                            semaphoreSlimMicrosoftGraph.Release();
                        }
                    });

                    return accessToken;

                }
                finally
                {
                    semaphoreSlimMicrosoftGraph.Release();
                }
            }
            else
            {
                log.LogInformation($"Returning token from cache for resource {resourceUri.DnsSafeHost} and user {userPrincipalName}");
                return accessTokenFromCache;
            }
        }

        private (string Username, string Password) GetCredential()
        {
            var username = string.Empty;
            var password = string.Empty;

            switch (configuration)
            {
                case OAuthCredentialManagerConfiguration credentialsManager:
                    // We're using a credential manager entry instead of a username/password set in the options
                    var credentials = CredentialManager.GetCredential(credentialsManager.CredentialManagerName);
                    username = credentials.UserName;
                    password = credentials.Password;
                    break;
                case OAuthUsernamePasswordConfiguration usernamePassword:
                    username = usernamePassword.Username;
                    password = usernamePassword.Password.ToInsecureString();
                    break;
                case OAuthCertificateConfiguration certificate:
                    // TODO: To implement ...
                    break;
            }

            return (username, password);
        }

        private static string TokenFromCache(Uri web, ConcurrentDictionary<string, string> tokenCache)
        {
            if (tokenCache.TryGetValue(web.DnsSafeHost, out string accessToken))
            {
                return accessToken;
            }

            return null;
        }

        private static void AddTokenToCache(Uri web, ConcurrentDictionary<string, string> tokenCache, string newAccessToken)
        {
            if (tokenCache.TryGetValue(web.DnsSafeHost, out string currentAccessToken))
            {
                tokenCache.TryUpdate(web.DnsSafeHost, newAccessToken, currentAccessToken);
            }
            else
            {
                tokenCache.TryAdd(web.DnsSafeHost, newAccessToken);
            }
        }

        private static void RemoveTokenFromCache(Uri web, ConcurrentDictionary<string, string> tokenCache)
        {
            tokenCache.TryRemove(web.DnsSafeHost, out string currentAccessToken);
        }

        private static TimeSpan CalculateThreadSleep(string accessToken)
        {
            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(accessToken);
            var lease = GetAccessTokenLease(token.ValidTo);
            lease = TimeSpan.FromSeconds(lease.TotalSeconds - TimeSpan.FromMinutes(5).TotalSeconds > 0 ? lease.TotalSeconds - TimeSpan.FromMinutes(5).TotalSeconds : lease.TotalSeconds);
            return lease;
        }

        private static TimeSpan GetAccessTokenLease(DateTime expiresOn)
        {
            DateTime now = DateTime.UtcNow;
            DateTime expires = expiresOn.Kind == DateTimeKind.Utc ? expiresOn : TimeZoneInfo.ConvertTimeToUtc(expiresOn);
            TimeSpan lease = expires - now;
            return lease;
        }
    }
}
