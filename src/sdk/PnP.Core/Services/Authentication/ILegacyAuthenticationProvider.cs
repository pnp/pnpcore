using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace PnP.Core.Services
{
    /// <summary>
    /// Defines the internal interface that a legacy Authentication Provider must implement
    /// </summary>
    internal interface ILegacyAuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Provides the value of the Cookie header for legacy cookie-based authentication
        /// </summary>
        /// <param name="targetUrl">The URL of the target API</param>
        /// <returns>The string value of the Cookie header to use for cookie authentication</returns>
        string GetCookieHeader(Uri targetUrl);

        /// <summary>
        /// Allows to see if the authentication provider should prioritize the cookie authentication
        /// </summary>
        public bool RequiresCookieAuthentication { get; }
    }
}
