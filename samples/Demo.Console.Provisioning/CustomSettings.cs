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
        /// Where extracted templates are written, relative to the executable.
        /// </summary>
        public string TemplateFolder { get; set; }
    }
}
