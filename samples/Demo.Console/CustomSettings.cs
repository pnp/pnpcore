using System;

namespace Demo.ConsoleApp
{
    public class CustomSettings
    {
        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string DemoSiteUrl { get; set; }
        public string DemoSubSiteUrl { get; set; }
        public string CredentialManager { get; set; }
        public Uri RedirectUri { get; set; }
    }
}
