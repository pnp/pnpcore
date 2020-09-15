using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// Configuration for using an existing access token 
    /// </summary>
    public class OAuthAccessTokenConfiguration : IAuthenticationProviderConfiguration
    {
        /// <summary>
        /// The Name of the configuration
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The ClientId of the application to use for authentication, will always be null when we get an extenral access token
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Defines the type of the IAuthenticationProvider to create
        /// </summary>
        public Type AuthenticationProviderType => typeof(OAuthAuthenticationProvider);

        /// <summary>
        /// The externally obtained access token to use
        /// </summary>
        public string AccessToken { get; set; }
    }
}
