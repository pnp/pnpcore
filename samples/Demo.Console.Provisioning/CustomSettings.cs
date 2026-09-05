namespace Demo.Console.Provisioning
{
    /// <summary>
    /// The settings this sample reads from <c>appsettings.json</c>.
    /// </summary>
    public class CustomSettings
    {
        /// <summary>
        /// The Entra application to sign in with. Defaults to the multi-tenant PnP application.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The tenant to sign in against, or <c>common</c> to be asked.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Where the interactive sign in returns to. Must match the application's registration.
        /// </summary>
        public string RedirectUri { get; set; }
        /// <summary>
        /// Thumbprint of a certificate in the certificate store. When set, the sample authenticates
        /// as the application instead of signing a user in.
        /// </summary>
        public string CertificateThumbprint { get; set; }

        /// <summary>
        /// Certificate store name holding the certificate. Defaults to My.
        /// </summary>
        public string CertificateStoreName { get; set; }

        /// <summary>
        /// Certificate store location holding the certificate. Defaults to CurrentUser.
        /// </summary>
        public string CertificateStoreLocation { get; set; }

        /// <summary>
        /// Where extracted templates are written, relative to the executable.
        /// </summary>
        public string TemplateFolder { get; set; }
    }
}
