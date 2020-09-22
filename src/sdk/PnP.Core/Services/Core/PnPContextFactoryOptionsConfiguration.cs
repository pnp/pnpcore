using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// PnPContext configuration options
    /// </summary>
    public class PnPContextFactoryOptionsConfiguration
    {
        /// <summary>
        /// The Name of the configuration
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The URL of the target SPO Site
        /// </summary>
        public Uri SiteUrl { get; set; }

        /// <summary>
        /// The Authentication Provider configuration
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider { get; set; }
    }
}
