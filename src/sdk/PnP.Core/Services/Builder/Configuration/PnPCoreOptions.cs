using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Builder.Configuration
{
    /// <summary>
    /// Options for configuring PnP Core SDK
    /// </summary>
    public class PnPCoreOptions 
    {
        /// <summary>
        /// The credentials options
        /// </summary>
        public PnPCoreCredentialsOptions Credentials { get; } = new PnPCoreCredentialsOptions();

        /// <summary>
        /// The sites options
        /// </summary>
        public PnPCoreSitesOptions Sites { get; } = new PnPCoreSitesOptions();
    }

    /// <summary>
    /// Options for configuring PnP Core SDK
    /// </summary>
    public class PnPCoreCredentialsOptions : Dictionary<String, PnPCoreCredentialOptions>
    {
    }

    /// <summary>
    /// Options for one configured credentials
    /// </summary>
    public class PnPCoreCredentialOptions
    {
        /// <summary>
        /// The ClientId of the application to use for authentication
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The name of the Credential Manager item to use, in case of CredentialManager Authentication
        /// </summary>
        public string CredentialManagerName { get; set; }

        /// <summary>
        /// The thumbprint of the X.509 Certificate to use, in case of X.509 Certificate Authentication
        /// </summary>
        public string CertificateThumbprint { get; set; }
    }

    /// <summary>
    /// Options for configuring PnP Core SDK
    /// </summary>
    public class PnPCoreSitesOptions : Dictionary<String, PnPCoreSiteOptions>
    {
    }

    /// <summary>
    /// Options for configuring a single site in PnP Core SDK
    /// </summary>
    public class PnPCoreSiteOptions
    {
        /// <summary>
        /// The URL of the target site
        /// </summary>
        public string SiteUrl { get; set; }

        /// <summary>
        /// The name of the Authentication Provider
        /// </summary>
        public string AuthenticationProviderName { get; set; }
    }
}
