using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Auth.Utilities;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace PnP.Core.Auth
{
    /// <summary>
    /// Authentication Provider based on the OnBehalfOf flow
    /// </summary>
    /// <remarks>
    /// You can find further details about the On-Behalf-Of flow here: https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow
    /// </remarks>
    public sealed class OnBehalfOfAuthenticationProvider : OAuthAuthenticationProvider
    {
        /// <summary>
        /// The X.509 Certificate to use for app authentication
        /// </summary>
        public X509Certificate2 Certificate { get; set; }

        /// <summary>
        /// The ClientSecret to authenticate the app with ClientId
        /// </summary>
        public SecureString ClientSecret { get; set; }

        /// <summary>
        /// A function providing the consumer user access token to use for the On-Behalf-Of flow
        /// </summary>
        public Func<string> UserTokenProvider { get; set; }

        // Instance private member, to keep the token cache at service instance level
        private IConfidentialClientApplication confidentialClientApplication;

        // Instance private member, to keep the Msal Http Client Factory at service instance level
        private readonly IMsalHttpClientFactory msalHttpClientFactory;

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenant ID for the Authentication Provider</param>
        /// <param name="clientSecret">The Client Secret of the app</param>
        /// <param name="userTokenProvider">A function providing the consumer user access token to use for the On-Behalf-Of flow</param>
        public OnBehalfOfAuthenticationProvider(string clientId, string tenantId,
            SecureString clientSecret,
            Func<string> userTokenProvider)
            : this(clientId, tenantId, new PnPCoreAuthenticationOnBehalfOfOptions
            {
                ClientSecret = clientSecret?.ToInsecureString()
            }, userTokenProvider)
        {
        }

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenant ID for the Authentication Provider</param>
        /// <param name="options">Options for the authentication provider</param>
        /// <param name="userTokenProvider">A function providing the consumer user access token to use for the On-Behalf-Of flow</param>
        public OnBehalfOfAuthenticationProvider(string clientId, string tenantId,
            PnPCoreAuthenticationOnBehalfOfOptions options,
            Func<string> userTokenProvider)
            : this(null, null)
        {
            UserTokenProvider = userTokenProvider;
            Init(new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = clientId,
                TenantId = tenantId,
                OnBehalfOf = options
            });
        }

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenant ID for the Authentication Provider</param>
        /// <param name="storeName">The Store Name to get the X.509 certificate from</param>
        /// <param name="storeLocation">The Store Location to get the X.509 certificate from</param>
        /// <param name="thumbprint">The Thumbprint of the X.509 certificate</param>
        /// <param name="userTokenProvider">A function providing the consumer user access token to use for the On-Behalf-Of flow</param>
        public OnBehalfOfAuthenticationProvider(string clientId, string tenantId,
            StoreName storeName, StoreLocation storeLocation, string thumbprint,
            Func<string> userTokenProvider)
            : this(null, null)
        {
            UserTokenProvider = userTokenProvider;
            Init(new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = clientId,
                TenantId = tenantId,
                OnBehalfOf = new PnPCoreAuthenticationOnBehalfOfOptions
                {
                    StoreName = storeName,
                    StoreLocation = storeLocation,
                    Thumbprint = thumbprint
                }
            });
        }

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenant ID for the Authentication Provider</param>
        /// <param name="certificate">The X.509 certificate to use for authentication</param>
        /// <param name="userTokenProvider">A function providing the consumer user access token to use for the On-Behalf-Of flow</param>
        public OnBehalfOfAuthenticationProvider(string clientId, string tenantId,
            X509Certificate2 certificate,
            Func<string> userTokenProvider)
            : this(null, null)
        {
            UserTokenProvider = userTokenProvider;
            Init(new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = clientId,
                TenantId = tenantId,
                OnBehalfOf = new PnPCoreAuthenticationOnBehalfOfOptions
                {
                    Certificate = certificate
                }
            });
        }

        /// <summary>
        /// Public constructor leveraging DI to initialize the ILogger and IMsalHttpClientFactory interfaces
        /// </summary>
        /// <param name="logger">The instance of the logger service provided by DI</param>
        /// <param name="msalHttpClientFactory">The instance of the Msal Http Client Factory service provided by DI</param>
        public OnBehalfOfAuthenticationProvider(ILogger<OAuthAuthenticationProvider> logger, IMsalHttpClientFactory msalHttpClientFactory)
            : base(logger)
        {
            this.msalHttpClientFactory = msalHttpClientFactory;
        }

        /// <summary>
        /// Initializes the Authentication Provider
        /// </summary>
        /// <param name="options">The options to use</param>
        internal override void Init(PnPCoreAuthenticationCredentialConfigurationOptions options)
        {
            // We need the OnBehalfOf options
            if (options.OnBehalfOf == null)
            {
                throw new ConfigurationErrorsException(
                    PnPCoreAuthResources.OnBehalfOfAuthenticationProvider_InvalidConfiguration);
            }

            if (options.OnBehalfOf.Certificate == null &&
                string.IsNullOrEmpty(options.OnBehalfOf.Thumbprint) &&
                string.IsNullOrEmpty(options.OnBehalfOf.ClientSecret))
            {
                throw new ConfigurationErrorsException(PnPCoreAuthResources.OnBehalfOfAuthenticationProvider_InvalidClientSecretOrCertificate);
            }

            ClientId = !string.IsNullOrEmpty(options.ClientId) ? options.ClientId : AuthGlobals.DefaultClientId;
            TenantId = !string.IsNullOrEmpty(options.TenantId) ? options.TenantId : AuthGlobals.OrganizationsTenantId;
            if (options.OnBehalfOf.Certificate != null)
            {
                // We prioritize the X.509 certificate, if any
                Certificate = options.OnBehalfOf.Certificate;
            }
            else if(!string.IsNullOrEmpty(options.OnBehalfOf.Thumbprint))
            {
                // We prioritize the X.509 certificate, if any
                Certificate = X509CertificateUtility.LoadCertificate(
                    options.OnBehalfOf.StoreName,
                    options.OnBehalfOf.StoreLocation,
                    options.OnBehalfOf.Thumbprint);
            }
            else if (!string.IsNullOrEmpty(options.OnBehalfOf.ClientSecret))
            {
                // Otherwise we fallback to the client secret
                ClientSecret = options.OnBehalfOf.ClientSecret.ToSecureString();
            }

            if (Certificate != null)
            {
                confidentialClientApplication = ConfidentialClientApplicationBuilder
                    .Create(ClientId)
                    .WithCertificate(Certificate)
                    .WithHttpClientFactory(msalHttpClientFactory)
                    .WithPnPAdditionalAuthenticationSettings(
                        options.OnBehalfOf.AuthorityUri,
                        options.OnBehalfOf.RedirectUri,
                        TenantId,
                        options.Environment,
                        options.AzureADLoginAuthority)
                    .Build();
            }
            else
            {
                confidentialClientApplication = ConfidentialClientApplicationBuilder
                    .Create(ClientId)
                    .WithClientSecret(ClientSecret.ToInsecureString())
                    .WithHttpClientFactory(msalHttpClientFactory)
                    .WithPnPAdditionalAuthenticationSettings(
                        options.OnBehalfOf.AuthorityUri,
                        options.OnBehalfOf.RedirectUri,
                        TenantId,
                        options.Environment,
                        options.AzureADLoginAuthority)
                    .Build();
            }

            // Log the initialization information
            Log?.LogInformation(PnPCoreAuthResources.OnBehalfOfAuthenticationProvider_LogInit);
        }

        /// <summary>
        /// Authenticates the specified request message.
        /// </summary>
        /// <param name="resource">Request uri</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        /// <returns>The task to await.</returns>
        public override async Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request)
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
        /// Gets an access token for the requested resource and scope
        /// </summary>
        /// <param name="resource">Resource to request an access token for (unused)</param>
        /// <param name="scopes">Scopes to request</param>
        /// <returns>An access token</returns>
        public override async Task<string> GetAccessTokenAsync(Uri resource, string[] scopes)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            if (UserTokenProvider == null)
            {
                throw new ConfigurationErrorsException(
                    PnPCoreAuthResources.OnBehalfOfAuthenticationProvider_MissingUserTokenProvider);
            }

            AuthenticationResult tokenResult = null;

            try
            {
                // Define the user assertion based on the consumer access token
                var assertion = new UserAssertion(UserTokenProvider.Invoke());

                // Try to get the token from the tokens cache
                tokenResult = await confidentialClientApplication
                    .AcquireTokenOnBehalfOf(scopes, assertion)
                    .ExecuteAsync().ConfigureAwait(false);
            }
            catch (MsalServiceException)
            {
                // Handle the various exceptions
                throw;
            }

            // Log the access token retrieval action
            Log?.LogDebug(PnPCoreAuthResources.AuthenticationProvider_LogAccessTokenRetrieval,
                GetType().Name, resource, scopes.Aggregate(string.Empty, (c, n) => c + ", " + n).TrimEnd(','));

            // Return the Access Token, if we've got it
            // In case of any exception while retrieving the access token, 
            // MSAL will throw an exception that we simply bubble up
            return tokenResult.AccessToken;
        }

        /// <summary>
        /// Gets an access token for the requested resource 
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <returns>An access token</returns>
        public override async Task<string> GetAccessTokenAsync(Uri resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            // Use the .default scope if the scopes are not provided
            return await GetAccessTokenAsync(resource,
                new string[] { $"{resource.Scheme}://{resource.Authority}/.default" }).ConfigureAwait(false);
        }
    }
}
