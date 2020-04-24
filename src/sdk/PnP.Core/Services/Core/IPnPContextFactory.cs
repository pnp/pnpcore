using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// Public interface for the injectable service to create an PnPContext
    /// </summary>
    public interface IPnPContextFactory
    {
        /// <summary>
        /// Creates a new instance of PnPContext based on a provided URL and Authentication configuration name
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="authenticationProviderName">The name of the Authentication Provider to use to authenticate within the PnPContext</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(Uri url, string authenticationProviderName);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided URL and Authentication Provider instance
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(Uri url, IAuthenticationProvider authenticationProvider);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided URL and using the default Authentication Provider
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(Uri url);

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The name of the configuration to use</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public PnPContext Create(string name);
    }
}
