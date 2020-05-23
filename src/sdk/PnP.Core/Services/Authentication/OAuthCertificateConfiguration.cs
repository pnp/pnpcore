using System;
using System.Security.Cryptography.X509Certificates;

namespace PnP.Core.Services
{
    /// <summary>
    /// Public type to define the Authentication based on X.509 Certificate
    /// </summary>
    public class OAuthCertificateConfiguration : IAuthenticationProviderConfiguration
    {
        /// <summary>
        /// The X.509 Certificate to use for authentication
        /// </summary>
        public X509Certificate2 Certificate { get; set; }

        /// <summary>
        /// The Name of the configuration
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The ClientId of the application to use for authentication
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Defines the type of the IAuthenticationProvider to create
        /// </summary>
        public Type AuthenticationProviderType => typeof(OAuthAuthenticationProvider);
    }
}
