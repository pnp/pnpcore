using System;
using System.Collections.Generic;
using System.Security;

namespace PnP.Core.Services
{
    /// <summary>
    /// Public type to define the Authentication with Username and Password
    /// </summary>
    public class OAuthUsernamePasswordConfiguration : IAuthenticationProviderConfiguration
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
        /// The username for authenticating
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password for authenticating
        /// </summary>
        public SecureString Password { get; set; }

        public void Init(Dictionary<string, string> options)
        {
            throw new NotImplementedException();
        }
    }
}
