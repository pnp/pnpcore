
namespace Demo.AzFunction.ManagedIdentity
{
    // To get an environment variable or an app setting value, use System.Environment.GetEnvironmentVariable
    // The System.Configuration.ConfigurationManager.AppSettings property is an alternative API for getting app setting values, but we recommend that you use GetEnvironmentVariable as shown here.
    // https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library?tabs=v2%2Ccmd#environment-variables
    public class AppConfig
    {
        public static string SiteUrl
        {
            get
            {
                string? url = Environment.GetEnvironmentVariable("SiteUrl");
                if (string.IsNullOrEmpty(url))
                {
                    throw new Exception("SiteUrl is not set in the environment variables");
                }
                return url!;
            }
        }

        // When MSI is enabled for an App Service, two environment variables MSI_ENDPOINT and MSI_SECRET are available
        public bool isMSI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MSI_SECRET"));
    }
    public class AppConfigCert
    {
        public static string ClientId
        {
            get
            {
                string? clientId = Environment.GetEnvironmentVariable("ClientId");
                if (string.IsNullOrEmpty(clientId))
                {
                    throw new Exception("ClientId is not set in the environment variables");
                }
                return clientId!;
            }
        }
        public static string TenantId
        {
            get
            {
                string? tenantId = Environment.GetEnvironmentVariable("TenantId");
                if (string.IsNullOrEmpty(tenantId))
                {
                    throw new Exception("TenantId is not set in the environment variables");
                }
                return tenantId!;
            }
        }
        public static string CertificateThumbprint
        {
            get
            {
                string? thumbprint = Environment.GetEnvironmentVariable("CertificateThumbprint");
                if (string.IsNullOrEmpty(thumbprint))
                {
                    throw new Exception("CertificateThumbprint is not set in the environment variables");
                }
                return thumbprint!;
            }
        }
    }
}