using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Services.Builder.Configuration;
using PnP.Core.Modernization.Services.Core;
using PnP.Core.Modernization.Services.MappingProviders;

namespace PnP.Core.Modernization.Test
{
    [TestClass]
    public class SetupTests
    {

        [TestMethod]
        public void ServicesConfiguration()
        {
            var services = new ServiceCollection();
            services.AddPnPModernization(o => o.DisableTelemetry = true)
                .WithPageOptions(o => o.DisablePageComments = true)
                .WithMetadataMappingProvider<ProvidersMock>()
                .WithPageLayoutMappingProvider<ProvidersMock>()
                .WithTaxonomyMappingProvider<ProvidersMock>()
                .WithUrlMappingProvider<ProvidersMock>()
                .WithUserMappingProvider<ProvidersMock>()
                .WithWebPartMappingProvider<ProvidersMock>();

            var provider = services.BuildServiceProvider();

            var pnpModernizationOptions = provider.GetRequiredService<IOptions<PnPModernizationOptions>>().Value;
            Assert.IsTrue(pnpModernizationOptions.DisableTelemetry);

            var transformationOptions = provider.GetRequiredService<IOptions<PageTransformationOptions>>().Value;
            Assert.IsTrue(transformationOptions.DisablePageComments);

            Assert.IsInstanceOfType(transformationOptions.MetadataMappingProvider, typeof(ProvidersMock));
            Assert.IsInstanceOfType(transformationOptions.PageLayoutMappingProvider, typeof(ProvidersMock));
            Assert.IsInstanceOfType(transformationOptions.TaxonomyMappingProvider, typeof(ProvidersMock));
            Assert.IsInstanceOfType(transformationOptions.UrlMappingProvider, typeof(ProvidersMock));
            Assert.IsInstanceOfType(transformationOptions.UserMappingProvider, typeof(ProvidersMock));
            Assert.IsInstanceOfType(transformationOptions.MetadataMappingProvider, typeof(ProvidersMock));
        }

        #region ProvidersMock

        private class ProvidersMock : IWebPartMappingProvider, IUserMappingProvider, ITaxonomyMappingProvider, IPageLayoutMappingProvider, IMetadataMappingProvider, IUrlMappingProvider
        {
            public Task<WebPartMappingProviderOutput> MapWebPartAsync(WebPartMappingProviderInput input)
            {
                throw new System.NotImplementedException();
            }

            public Task<UserMappingProviderOutput> MapUserAsync(UserMappingProviderInput input)
            {
                throw new System.NotImplementedException();
            }

            public Task<TaxonomyMappingProviderOutput> MapTermAsync(TaxonomyMappingProviderInput input)
            {
                throw new System.NotImplementedException();
            }

            public Task<PageLayoutMappingProviderOutput> MapPageLayoutAsync(PageLayoutMappingProviderInput input)
            {
                throw new System.NotImplementedException();
            }

            public Task<MetadataMappingProviderOutput> MapMetadataFieldAsync(MetadataMappingProviderInput input)
            {
                throw new System.NotImplementedException();
            }

            public Task<UrlMappingProviderOutput> MapUrlAsync(UrlMappingProviderInput input)
            {
                throw new System.NotImplementedException();
            }
        }

        #endregion 
    }
}
