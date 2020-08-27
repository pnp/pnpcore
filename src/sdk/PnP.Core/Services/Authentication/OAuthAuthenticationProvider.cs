using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PnP.Core.Services
{
    /// <summary>
    /// OAuth authentication provider, uses Azure AD to authenticate requests and provide access tokens
    /// </summary>
    public class OAuthAuthenticationProvider : IAuthenticationProvider, IDisposable
    {
        private bool disposedValue;

        private static readonly HttpClient httpClient = new HttpClient();
        private const string tokenEndpoint = "https://login.microsoftonline.com/common/oauth2/token";

        // Microsoft SharePoint Online Management Shell client id
        // private static readonly string aadAppId = "9bc3ab49-b65d-410a-85ad-de819febfddc";
        // PnP Office 365 Management Shell 
        private const string defaultAADAppId = "31359c7f-bd7e-475c-86db-fdb8c937548e";

        private readonly ILogger log;
        private readonly IOAuthAccessTokenProvider accessTokenProvider;

        // Token cache handling
        private static readonly SemaphoreSlim semaphoreSlimTokens = new SemaphoreSlim(1);
        private AutoResetEvent tokenResetEvent = null;
        private readonly ConcurrentDictionary<string, string> tokenCache = new ConcurrentDictionary<string, string>();

        internal class TokenWaitInfo
        {
            public RegisteredWaitHandle Handle = null;
        }

        /// <summary>
        /// Get's the in use <see cref="IAuthenticationProviderConfiguration"/>
        /// </summary>
        public IAuthenticationProviderConfiguration Configuration { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="accessTokenProvider">Provder for delivering an access token</param>
        public OAuthAuthenticationProvider(ILogger<OAuthAuthenticationProvider> logger, IOAuthAccessTokenProvider accessTokenProvider = null)
        {
            log = logger;
            this.accessTokenProvider = accessTokenProvider;
        }

        /// <summary>
        /// Configure this authentication provider for the desired configuration
        /// </summary>
        /// <param name="configuration"><see cref="IAuthenticationProviderConfiguration"/> (e.g. <see cref="OAuthCredentialManagerConfiguration"/>) to use</param>
        public void Configure(IAuthenticationProviderConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Authenticate a given request by adding the needed authorization header
        /// </summary>
        /// <param name="resource">Resource to authenticate against</param>
        /// <param name="request"><see cref="HttpRequestMessage"/> to update with authentication details</param>
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

            switch (Configuration)
            {
                case OAuthCredentialManagerConfiguration _:
                case OAuthUsernamePasswordConfiguration _:
                    {
                        var (Username, Password) = GetCredential();
                        if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", await EnsureSharePointAccessTokenAsync(resource, Username, Password).ConfigureAwait(false));
                        }
                        break;
                    }
                case OAuthCertificateConfiguration _:
                    // TODO: To implement ...
                    break;
                case OAuthAccessTokenConfiguration accessTokenConfig:
                    {
                        string accessToken = "";
                        // Did we set an access token on the config?
                        if (!string.IsNullOrEmpty(accessTokenConfig.AccessToken))
                        {
                            accessToken = accessTokenConfig.AccessToken;
                        }
                        // Do we have an external access token provider?
                        else if (accessTokenProvider != null)
                        {
                            accessToken = await accessTokenProvider.GetAccessTokenAsync(resource).ConfigureAwait(false);
                        }

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                        }

                        break;
                    }
            }
        }

        private async Task<string> GetMicrosoftGraphAccessTokenAsync()
        {
            switch (Configuration)
            {
                case OAuthCredentialManagerConfiguration _:
                case OAuthUsernamePasswordConfiguration _:
                    {
                        var (Username, Password) = GetCredential();
                        if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                        {
                            return await EnsureMicrosoftGraphAccessTokenAsync(Username, Password).ConfigureAwait(false);
                        }
                        break;
                    }
                case OAuthCertificateConfiguration _:
                    // TODO: To implement ...
                    break;
                case OAuthAccessTokenConfiguration accessTokenConfig:
                    {
                        if (!string.IsNullOrEmpty(accessTokenConfig.AccessToken))
                        {
                            return accessTokenConfig.AccessToken;
                        }
                        if (accessTokenProvider != null)
                        {
                            string accessToken = await accessTokenProvider.GetAccessTokenAsync(PnPConstants.MicrosoftGraphBaseUri).ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                return accessToken;
                            }
                        }
                        break;
                    }
            }

            return null;
        }

        private async Task<string> GetSharePointOnlineAccessTokenAsync(Uri resource)
        {
            switch (Configuration)
            {
                case OAuthCredentialManagerConfiguration _:
                case OAuthUsernamePasswordConfiguration _:
                    {
                        var (Username, Password) = GetCredential();
                        if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                        {
                            return await EnsureSharePointAccessTokenAsync(resource, Username, Password).ConfigureAwait(false);
                        }
                        break;
                    }
                case OAuthCertificateConfiguration _:
                    // TODO: To implement ...
                    break;
                case OAuthAccessTokenConfiguration accessTokenConfig:
                    {
                        if (!string.IsNullOrEmpty(accessTokenConfig.AccessToken))
                        {
                            return accessTokenConfig.AccessToken;
                        }
                        if (accessTokenProvider != null)
                        {
                            string accessToken = await accessTokenProvider.GetAccessTokenAsync(resource).ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                return accessToken;
                            }
                        }
                        break;
                    }
            }

            return null;
        }

        /// <summary>
        /// Get an access token for the requested resource 
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <returns></returns>
        public async Task<string> GetAccessTokenAsync(Uri resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            return await GetAccessTokenAsync(resource, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Get an access token for the requested resource and scope(s)
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <param name="scopes">Scope(s) to be used for the access token request</param>
        /// <returns></returns>
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
            else if (resource.AbsoluteUri.ToLower(CultureInfo.InvariantCulture).Contains("sharepoint.com"))
            {
                return await GetSharePointOnlineAccessTokenAsync(resource).ConfigureAwait(false);
            }
            else
            {
                return default(string);
            }
        }

        private async Task<string> AcquireTokenAsync(Uri resourceUri, string username, string password)
        {
            string resource = $"{resourceUri.Scheme}://{resourceUri.DnsSafeHost}";

            var clientId = this.Configuration.ClientId ?? defaultAADAppId;
            var body = $"resource={resource}&client_id={clientId}&grant_type=password&username={HttpUtility.UrlEncode(username)}&password={HttpUtility.UrlEncode(password)}";
            using (var stringContent = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"))
            {
                var response = await httpClient.PostAsync(tokenEndpoint, stringContent).ConfigureAwait(false);
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                // Inspect the response, a valid response contains the access_token we need. Carriage returns in a string make parsing fail (regression?), remove them to be at the safe side
                var tokenResult = JsonSerializer.Deserialize<JsonElement>(result.Replace("\r\n", " ").Trim());
                string token = null;
                if (tokenResult.TryGetProperty("access_token", out JsonElement accessToken))
                {
                    token = accessToken.GetString();
                }
                else
                {
                    // Oops, seems something went wrong
                    throw new AuthenticationException(ErrorType.AzureADError, result);
                }

                return token;
            }
        }

        private async Task<string> EnsureSharePointAccessTokenAsync(Uri resourceUri, string userPrincipalName, string userPassword)
        {
            return await EnsureAccessTokenAsync(resourceUri, userPrincipalName, userPassword).ConfigureAwait(true);
        }

        private async Task<string> EnsureMicrosoftGraphAccessTokenAsync(string userPrincipalName, string userPassword)
        {
            Uri resourceUri = PnPConstants.MicrosoftGraphBaseUri;
            return await EnsureAccessTokenAsync(resourceUri, userPrincipalName, userPassword).ConfigureAwait(true);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async Task<string> EnsureAccessTokenAsync(Uri resourceUri, string userPrincipalName, string userPassword)
        {
            string accessTokenFromCache = TokenFromCache(resourceUri, tokenCache);
            if (accessTokenFromCache == null)
            {
                await semaphoreSlimTokens.WaitAsync().ConfigureAwait(false);
                try
                {
                    // No async methods are allowed in a lock section
                    string accessToken = await AcquireTokenAsync(resourceUri, userPrincipalName, userPassword).ConfigureAwait(false);
                    log.LogInformation($"Successfully requested new access token resource {resourceUri.DnsSafeHost} and user {userPrincipalName}");
                    AddTokenToCache(resourceUri, tokenCache, accessToken);

                    // Register a thread to invalidate the access token once's it's expired
                    tokenResetEvent = new AutoResetEvent(false);
                    TokenWaitInfo wi = new TokenWaitInfo();
                    wi.Handle = ThreadPool.RegisterWaitForSingleObject(
                        tokenResetEvent,
                        async (state, timedOut) =>
                        {
                            if (!timedOut)
                            {
                                TokenWaitInfo wi = (TokenWaitInfo)state;
                                if (wi.Handle != null)
                                {
                                    wi.Handle.Unregister(null);
                                }
                            }
                            else
                            {
                                try
                                {
                                    // Take a lock to ensure no other threads are updating the SharePoint Access token at this time
                                    await semaphoreSlimTokens.WaitAsync().ConfigureAwait(false);
                                    RemoveTokenFromCache(resourceUri, tokenCache);
                                    log.LogInformation($"Cached token for resource {resourceUri.DnsSafeHost} and user {userPrincipalName} expired");
                                }
                                catch (Exception ex)
                                {
                                    log.LogError(ex, $"Something went wrong during cache token invalidation: {ex.Message}");
                                    RemoveTokenFromCache(resourceUri, tokenCache);
                                }
                                finally
                                {
                                    semaphoreSlimTokens.Release();
                                }
                            }
                        },
                        wi,
                        (uint)CalculateThreadSleep(accessToken).TotalMilliseconds,
                        true
                    );

                    return accessToken;
                }
                finally
                {
                    semaphoreSlimTokens.Release();
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

            switch (Configuration)
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
                case OAuthCertificateConfiguration _:
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

        /// <summary>
        /// Disposes this <see cref="OAuthAuthenticationProvider"/>
        /// <param name="disposing">Do we need to dispose resources</param>
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (tokenResetEvent != null)
                    {
                        tokenResetEvent.Set();
                        tokenResetEvent.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes this <see cref="OAuthAuthenticationProvider"/>
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
