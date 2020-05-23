using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// Basic interface for all the Authentication Provider configurations
    /// </summary>
    public interface IAuthenticationProviderConfiguration
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
        public Type AuthenticationProviderType { get; }
    }
}
