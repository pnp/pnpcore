using System;

namespace Demo.AzFunction.ManagedIdentity
{
    // To get an environment variable or an app setting value, use System.Environment.GetEnvironmentVariable
    // The System.Configuration.ConfigurationManager.AppSettings property is an alternative API for getting app setting values, but we recommend that you use GetEnvironmentVariable as shown here.
    // https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library?tabs=v2%2Ccmd#environment-variables
    public class AppConfig
    {
        public string SiteUrl = Environment.GetEnvironmentVariable("SiteUrl");
        // When MSI is enabled for an App Service, two environment variables MSI_ENDPOINT and MSI_SECRET are available
        public bool isMSI = !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("MSI_SECRET"));
    }
    public class AppConfigCert
    {
        public string ClientId = Environment.GetEnvironmentVariable("ClientId");
        public string TenantId = Environment.GetEnvironmentVariable("TenantId");
        public string CertificateThumbprint = Environment.GetEnvironmentVariable("CertificateThumbprint");
    }
}