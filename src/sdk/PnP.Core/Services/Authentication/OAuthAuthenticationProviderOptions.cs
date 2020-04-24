using System.Collections.Generic;

namespace PnP.Core.Services
{
    /// <summary>
    /// Defines a collection of credential settings for secure connections to the target resources
    /// </summary>
    public class OAuthAuthenticationProviderOptions
    {
        /// <summary>
        /// Defines the name of the default configuration
        /// </summary>
        public string DefaultConfiguration { get; set; }

        /// <summary>
        /// Collection of credentials for OAuthAuthenticationProvider
        /// </summary>
        public List<IAuthenticationProviderConfiguration> Configurations { get; } = new List<IAuthenticationProviderConfiguration>();
    }
}
