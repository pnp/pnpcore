using System;

namespace PnP.Core.Services
{
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
        /// The Name of the Authentication Provider configuration
        /// </summary>
        public string AuthenticationProviderName { get; set; }
    }
}
