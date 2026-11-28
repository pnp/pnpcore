using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth;

using PnP.Core.Modernization.Services;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace PnP.Core.Modernization.Tests
{
    static class TestCommon
    {
        private static readonly Lazy<ServiceProvider> services = new(() =>
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddPnPCore(options => options.PnPContext.GraphFirst = false);
            return collection.BuildServiceProvider();
        });

        private static readonly Lazy<XDocument> configuration = new(() =>
        {
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "App.config"));
            return File.Exists(path) ? XDocument.Load(path) : null;
        });

        public static string InconclusiveNoAutomatedChecksMessage =>
            "Does not yet have automated checks, please manually check the results of the test";

        public static PnPContext CreateSPPlatformClientContext(SPPlatformVersion version, TransformType transformType)
        {
            if (version != SPPlatformVersion.SPO)
                throw new NotSupportedException("Modernization supports SharePoint Online only.");
            return CreateClientContext(AppSetting(transformType == TransformType.PublishingPage ? "SPOPublishingSite" : "SPOTeamSite"));
        }

        public static PnPContext CreateClientContext() => CreateClientContext(AppSetting("SPODevSiteUrl"));
        public static PnPContext CreateTenantClientContext() => CreateClientContext(AppSetting("SPOTenantUrl"));

        public static PnPContext CreateClientContext(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                Assert.Inconclusive("Configure the SharePoint Online test site URL.");
            SharePointOnlineSupport.Validate(new Uri(url));
            var clientId = RequiredSetting("PNP_MODERNIZATION_CLIENT_ID");
            var tenantId = RequiredSetting("PNP_MODERNIZATION_TENANT_ID");
            var thumbprint = RequiredSetting("PNP_MODERNIZATION_CERTIFICATE_THUMBPRINT");
            var authentication = new X509CertificateAuthenticationProvider(clientId, tenantId,
                StoreName.My, StoreLocation.CurrentUser, thumbprint);
            return services.Value.GetRequiredService<IPnPContextFactory>().Create(new Uri(url), authentication);
        }

        public static string AppSetting(string key) =>
            configuration.Value?.Root?.Element("appSettings")?.Elements("add")
                .SingleOrDefault(element => (string)element.Attribute("key") == key)?.Attribute("value")?.Value;

        private static string RequiredSetting(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
                Assert.Inconclusive($"Configure {name} for certificate-authenticated SharePoint Online integration tests.");
            return value;
        }

        public static SPPlatformVersion GetVersion(PnPContext context)
        {
            SharePointOnlineSupport.Validate(context);
            return SPPlatformVersion.SPO;
        }

        public static string UpdatePageToIncludeVersion(SPPlatformVersion version, string fileName)
        {
            if (version != SPPlatformVersion.SPO)
                throw new NotSupportedException("Modernization supports SharePoint Online only.");
            return fileName.Replace(".aspx", "-SPO.aspx");
        }

        internal static void DisposeServices()
        {
            if (services.IsValueCreated) services.Value.Dispose();
        }
    }

    [TestClass]
    public class TestAssemblyLifecycle
    {
        [AssemblyCleanup]
        public static void Cleanup()
        {
            CoreTestContext.DisposeResources();
            TestCommon.DisposeServices();
        }
    }

    public enum SPPlatformVersion
    {
        SP2010,
        SP2013,
        SP2016,
        SP2019,
        SPO
    }

    public enum TransformType
    {
        PublishingPage,
        WebPartPage,
        WikiPage
    }
}
