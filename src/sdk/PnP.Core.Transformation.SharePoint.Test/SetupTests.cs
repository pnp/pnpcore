using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.MappingProviders;

namespace PnP.Core.Transformation.SharePoint.Test
{
    [TestClass]
    public class SetupTests
    {

        [TestMethod]
        public void DefaultSharePointServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddPnPSharePointTransformation();

            var provider = services.BuildServiceProvider();

            // TODO: check all types

            Assert.IsInstanceOfType(provider.GetRequiredService<IMappingProvider>(), typeof(SharePointMappingProvider));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITransformationDistiller>(), typeof(SharePointTransformationDistiller));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITargetPageUriResolver>(), typeof(SharePointTargetPageUriResolver));

            Assert.IsInstanceOfType(provider.GetRequiredService<IMetadataMappingProvider>(), typeof(SharePointMetadataMappingProvider));
            Assert.IsInstanceOfType(provider.GetRequiredService<IHtmlMappingProvider>(), typeof(SharePointHtmlMappingProvider));
            Assert.IsInstanceOfType(provider.GetRequiredService<IPageLayoutMappingProvider>(), typeof(SharePointPageLayoutMappingProvider));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITaxonomyMappingProvider>(), typeof(SharePointTaxonomyMappingProvider));
            Assert.IsInstanceOfType(provider.GetRequiredService<IUserMappingProvider>(), typeof(SharePointUserMappingProvider));
            Assert.IsInstanceOfType(provider.GetRequiredService<IUrlMappingProvider>(), typeof(SharePointUrlMappingProvider));
            Assert.IsInstanceOfType(provider.GetRequiredService<IWebPartMappingProvider>(), typeof(SharePointWebPartMappingProvider));
        }

    }
}
