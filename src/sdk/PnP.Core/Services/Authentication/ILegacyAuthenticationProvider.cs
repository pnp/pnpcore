using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// Defines the interface that a legacy Authentication Provider must implement.
    /// Note: this is only meant to be used by PnP Framework, no 3rd party support will be provided.
    /// </summary>
    public interface ILegacyAuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Provides the value of the Cookie header for legacy cookie-based authentication
        /// </summary>
        /// <param name="targetUrl">The URL of the target API</param>
        /// <returns>The string value of the Cookie header to use for cookie authentication</returns>
        string GetCookieHeader(Uri targetUrl);

        /// <summary>
        /// Provides the value of the Request Digest header for legacy cookie-based authentication
        /// </summary>
        /// <returns>The string value of the Request Digest header to use for cookie authentication</returns>
        string GetRequestDigest();

        /// <summary>
        /// Allows to see if the authentication provider should prioritize the cookie authentication
        /// </summary>
        public bool RequiresCookieAuthentication { get; }
    }
}
