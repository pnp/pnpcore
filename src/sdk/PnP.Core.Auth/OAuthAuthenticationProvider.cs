using Microsoft.Extensions.Logging;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Auth
{
    public abstract class OAuthAuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// The Name of the configuration for the Authentication Provider
        /// </summary>
        public string ConfigurationName { get; set; }

        /// <summary>
        /// The ClientId of the application to use for authentication
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The Tenand ID for the application, default value is "organizations" for multi-tenant applications
        /// </summary>
        public string TenantId { get; set; }

        // Microsoft SharePoint Online Management Shell client id
        // private static readonly string aadAppId = "9bc3ab49-b65d-410a-85ad-de819febfddc";
        // PnP Office 365 Management Shell 
        protected const string DefaultAADAppId = "31359c7f-bd7e-475c-86db-fdb8c937548e";

        /// <summary>
        /// Local copy of the logger class
        /// </summary>
        protected readonly ILogger Log;

        ///// <summary>
        ///// Local copy of the configuration options
        ///// </summary>
        //protected AuthenticationProvidersOptions Options { get; private set; }

        public OAuthAuthenticationProvider(ILogger<OAuthAuthenticationProvider> logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            //if (options == null)
            //{
            //    throw new ArgumentNullException(nameof(options));
            //}

            Log = logger;
            //Options = options.Value;
        }

        /// <summary>
        /// Initializes the Authentication Provider
        /// </summary>
        public abstract void Init(PnPCoreAuthenticationCredentialConfigurationOptions options);

        /// <summary>
        /// Authenticates the specified request message.
        /// </summary>
        /// <param name="resource">Request uri</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        /// <returns>The task to await.</returns>
        public abstract Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request);

        /// <summary>
        /// Gets an access token for the requested resource and scope
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <param name="scopes">Scopes to request</param>
        /// <returns>An access token</returns>
        public abstract Task<string> GetAccessTokenAsync(Uri resource, string[] scopes);

        /// <summary>
        /// Gets an access token for the requested resource 
        /// </summary>
        /// <param name="resource">Resource to request an access token for</param>
        /// <returns>An access token</returns>
        public abstract Task<string> GetAccessTokenAsync(Uri resource);
    }
}
