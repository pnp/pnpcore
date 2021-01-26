using System;

namespace PnP.Core.Auth.Services
{
    /// <summary>
    /// Basic interface for all the Authentication Provider configurations
    /// </summary>
    public interface IAuthenticationProviderOptions
    {
        /// <summary>
        /// The ClientId of the application to use for authentication
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The Tenant ID for the application, can be "organizations" for multi-tenant applications
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Defines the type of the IAuthenticationProvider to create
        /// </summary>
        public Type AuthenticationProviderType { get; }
    }
}
