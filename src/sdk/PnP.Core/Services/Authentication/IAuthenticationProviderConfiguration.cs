using System;
using System.Collections.Generic;

namespace PnP.Core.Services
{
    /// <summary>
    /// Basic interface for all the Authentication Provider configurations
    /// </summary>
    public interface IAuthenticationProviderConfiguration
    {
        /// <summary>
        /// Method to initialize the configuration
        /// </summary>
        /// <param name="options">The options to initialize the configuration</param>
        public void Init(Dictionary<string, string> options);

        /// <summary>
        /// The Name of the configuration
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The ClientId of the application to use for authentication
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The Tenand ID for the application, can be "organizations" for multi-tenant applications
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Defines the type of the IAuthenticationProvider to create
        /// </summary>
        public Type AuthenticationProviderType { get; }
    }
}
