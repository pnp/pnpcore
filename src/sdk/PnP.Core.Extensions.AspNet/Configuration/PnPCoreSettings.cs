using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Extensions.AspNet.Configuration
{
    /// <summary>
    /// Options for configuring PnP Core SDK
    /// </summary>
    public class PnPCoreSettings
    {
        /// <summary>
        /// The URL of the target site
        /// </summary>
        public string SiteUrl { get; set; }

        /// <summary>
        /// The name of the Credential Manager account
        /// </summary>
        public string CredentialManager { get; set; }

        /// <summary>
        /// The thumbprint of the X.509 Certificate for app-only authentication
        /// </summary>
        public string X509CertificateThumbprint { get; set; }
    }
}
