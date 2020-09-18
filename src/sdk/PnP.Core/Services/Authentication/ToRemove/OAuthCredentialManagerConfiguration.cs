using System;
using System.Collections.Generic;

namespace PnP.Core.Services
{
    /// <summary>
    /// Public type to define the Authentication based on Credential Manager
    /// </summary>
    public class OAuthCredentialManagerConfiguration : IAuthenticationProviderConfiguration
    {
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

        /// <summary>
        /// The name of the Windows Credential Manager settings to use
        /// </summary>
        public string CredentialManagerName { get; set; }

        public void Init(Dictionary<string, string> options)
        {
            throw new NotImplementedException();
        }
    }
}
