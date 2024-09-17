using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Auth.Utilities;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace PnP.Core.Auth
{
    /// <summary>
    /// Authentication Provider based on the X.509 Certificate
    /// </summary>
    public sealed class X509CertificateAuthenticationProvider : OAuthAuthenticationProvider
    {
        /// <summary>
        /// The X.509 Certificate to use for app authentication
        /// </summary>
        public X509Certificate2 Certificate { get; set; }

        // Instance private member, to keep the token cache at service instance level
        private IConfidentialClientApplication confidentialClientApplication;

        // Instance private member, to keep the Msal Http Client Factory at service instance level
        private readonly IMsalHttpClientFactory msalHttpClientFactory;

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenant ID for the Authentication Provider</param>
        /// <param name="storeName">The Store Name to get the X.509 certificate from</param>
        /// <param name="storeLocation">The Store Location to get the X.509 certificate from</param>
        /// <param name="thumbprint">The Thumbprint of the X.509 certificate</param>
        public X509CertificateAuthenticationProvider(string clientId, string tenantId,
            StoreName storeName, StoreLocation storeLocation, string thumbprint)
            : this(clientId, tenantId,
                 new PnPCoreAuthenticationX509CertificateOptions
                 {
                     StoreName = storeName,
                     StoreLocation = storeLocation,
                     Thumbprint = thumbprint
                 })
        {
        }

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenant ID for the Authentication Provider</param>
        /// <param name="certificate">The X.509 certificate to use for authentication</param>
        public X509CertificateAuthenticationProvider(string clientId, string tenantId,
            X509Certificate2 certificate)
            : this(clientId, tenantId,
                 new PnPCoreAuthenticationX509CertificateOptions
                 {
                     Certificate = certificate
                 })
        {
        }

        /// <summary>
        /// Public constructor for external consumers of the library
        /// </summary>
        /// <param name="clientId">The Client ID for the Authentication Provider</param>
        /// <param name="tenantId">The Tenant ID for the Authentication Provider</param>
        /// <param name="options">Options for the authentication provider</param>
        public X509CertificateAuthenticationProvider(string clientId, string tenantId, PnPCoreAuthenticationX509CertificateOptions options)
            : this(null, null)
        {
            Init(new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = clientId,
                TenantId = tenantId,
                X509Certificate = options
            });
        }

        /// <summary>
        /// Public constructor leveraging DI to initialize the ILogger and IMsalHttpClientFactory interfaces
        /// </summary>
        /// <param name="logger">The instance of the logger service provided by DI</param>
        /// <param name="msalHttpClientFactory">The instance of the Msal Http Client Factory service provided by DI</param>
        public X509CertificateAuthenticationProvider(ILogger<OAuthAuthenticationProvider> logger, IMsalHttpClientFactory msalHttpClientFactory)
            : base(logger)
        {
            this.msalHttpClientFactory = msalHttpClientFactory;
        }

        /// <summary>
        /// Initializes the X509Certificate Authentication Provider
        /// </summary>
        /// <param name="options">The options to use</param>
        internal override void Init(PnPCoreAuthenticationCredentialConfigurationOptions options)
        {
            // We need the X509Certificate options
            if (options.X509Certificate == null)
            {
                throw new ConfigurationErrorsException(
                    PnPCoreAuthResources.X509CertificateAuthenticationProvider_InvalidConfiguration);
            }

            // We need the certificate thumbprint
            if (options.X509Certificate.Certificate == null && string.IsNullOrEmpty(options.X509Certificate.Thumbprint))
            {
                throw new ConfigurationErrorsException(PnPCoreAuthResources.X509CertificateAuthenticationProvider_InvalidCertificateOrThumbprint);
            }

            if (!string.IsNullOrEmpty(options.ClientId))
            {
                ClientId = options.ClientId;
            }
            else
            {
                throw new ConfigurationErrorsException(PnPCoreAuthResources.InvalidClientId);
            }

            TenantId = !string.IsNullOrEmpty(options.TenantId) ? options.TenantId : AuthGlobals.OrganizationsTenantId;
            Certificate = options.X509Certificate.Certificate ?? X509CertificateUtility.LoadCertificate(
                options.X509Certificate.StoreName,
                options.X509Certificate.StoreLocation,
                options.X509Certificate.Thumbprint);

            confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(ClientId)
                .WithCertificate(Certificate)
                .WithHttpClientFactory(msalHttpClientFactory)
                .WithPnPAdditionalAuthenticationSettings(
                    options.X509Certificate.AuthorityUri,
                    options.X509Certificate.RedirectUri,
                    TenantId,
                    options.Environment,
                    options.AzureADLoginAuthority)
                .Build();

            // Log the initialization information
            Log?.LogInformation(PnPCoreAuthResources.X509CertificateAuthenticationProvider_LogInit,
                options.X509Certificate.Thumbprint,
                options.X509Certificate.StoreName,
                options.X509Certificate.StoreLocation);
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

            AuthenticationResult tokenResult = null;

            try
            {
                // Try to get the token from the tokens cache
                tokenResult = await confidentialClientApplication.AcquireTokenForClient(scopes).ExecuteAsync().ConfigureAwait(false);
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
