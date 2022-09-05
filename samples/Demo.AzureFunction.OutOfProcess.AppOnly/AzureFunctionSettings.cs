using System.Security.Cryptography.X509Certificates;

namespace ProvisioningDemo
{
    internal class AzureFunctionSettings
    {
        public string SiteUrl { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public StoreName CertificateStoreName { get; set; }
        public StoreLocation CertificateStoreLocation { get; set; }
        public string CertificateThumbprint { get; set; }
    }
}
