using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Services.Builder.Configuration;
using PnP.Core.Modernization.Services.Core;
using PnP.Core.Modernization.Services.MappingProviders;
using PnP.Core.Services;

namespace PnP.Core.Modernization.Test
{
    [TestClass]
    public class SetupTests
    {

        [TestMethod]
        public void DefaultServices()
        {
            var services = new ServiceCollection();
            services.AddPnPModernization();

            var provider = services.BuildServiceProvider();

            //Assert.IsInstanceOfType(provider.GetRequiredService<IPageTransformator>(), typeof(Mock));
            //Assert.IsInstanceOfType(provider.GetRequiredService<ITransformationDistiller>(), typeof(Mock));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITransformationStateManager>(), typeof(InMemoryTransformationStateManager));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITransformationExecutor>(), typeof(SimpleTransformationExecutor));
        }

        [TestMethod]
        public void CustomServices()
        {
            var services = new ServiceCollection();
            services.AddPnPModernization(o => o.DisableTelemetry = true)
                .WithPageOptions(o => o.DisablePageComments = true)

                .WithMetadataMappingProvider<Mock>()
                .WithPageLayoutMappingProvider<Mock>()
                .WithTaxonomyMappingProvider<Mock>()
                .WithUrlMappingProvider<Mock>()
                .WithUserMappingProvider<Mock>()
                .WithWebPartMappingProvider<Mock>()

                .WithPageTransformator<Mock>()
                .WithTransformationDistiller<Mock>()
                .WithTransformationStateManager<Mock>()
                .WithTransformationExecutor<Mock>();

            var provider = services.BuildServiceProvider();

            var pnpModernizationOptions = provider.GetRequiredService<IOptions<PnPModernizationOptions>>().Value;
            Assert.IsTrue(pnpModernizationOptions.DisableTelemetry);

            var transformationOptions = provider.GetRequiredService<IOptions<PageTransformationOptions>>().Value;
            Assert.IsTrue(transformationOptions.DisablePageComments);

            Assert.IsInstanceOfType(transformationOptions.MetadataMappingProvider, typeof(Mock));
            Assert.IsInstanceOfType(transformationOptions.PageLayoutMappingProvider, typeof(Mock));
            Assert.IsInstanceOfType(transformationOptions.TaxonomyMappingProvider, typeof(Mock));
            Assert.IsInstanceOfType(transformationOptions.UrlMappingProvider, typeof(Mock));
            Assert.IsInstanceOfType(transformationOptions.UserMappingProvider, typeof(Mock));
            Assert.IsInstanceOfType(transformationOptions.MetadataMappingProvider, typeof(Mock));

            Assert.IsInstanceOfType(provider.GetRequiredService<IPageTransformator>(), typeof(Mock));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITransformationDistiller>(), typeof(Mock));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITransformationStateManager>(), typeof(Mock));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITransformationExecutor>(), typeof(Mock));
        }

        #region Mock

        private class Mock :
            IWebPartMappingProvider,
            IUserMappingProvider,
            ITaxonomyMappingProvider,
            IPageLayoutMappingProvider,
            IMetadataMappingProvider,
            IUrlMappingProvider,
            IPageTransformator,
            ITransformationDistiller,
            ITransformationStateManager,
            ITransformationExecutor
        {
            Task<WebPartMappingProviderOutput> IWebPartMappingProvider.MapWebPartAsync(WebPartMappingProviderInput input)
            {
                throw new System.NotImplementedException();
            }

            Task<UserMappingProviderOutput> IUserMappingProvider.MapUserAsync(UserMappingProviderInput input)
            {
                throw new System.NotImplementedException();
            }

            Task<TaxonomyMappingProviderOutput> ITaxonomyMappingProvider.MapTermAsync(TaxonomyMappingProviderInput input)
            {
                throw new System.NotImplementedException();
            }

            Task<PageLayoutMappingProviderOutput> IPageLayoutMappingProvider.MapPageLayoutAsync(PageLayoutMappingProviderInput input)
            {
                throw new System.NotImplementedException();
            }

            Task<MetadataMappingProviderOutput> IMetadataMappingProvider.MapMetadataFieldAsync(MetadataMappingProviderInput input)
            {
                throw new System.NotImplementedException();
            }

            Task<UrlMappingProviderOutput> IUrlMappingProvider.MapUrlAsync(UrlMappingProviderInput input)
            {
                throw new System.NotImplementedException();
            }

            Task<Uri> IPageTransformator.TransformAsync(PageTransformationTask task, Action<PageTransformationOptions> options = null)
            {
                throw new NotImplementedException();
            }

            Task<IAsyncEnumerable<PageTransformationTask>> ITransformationDistiller.GetTransformationTasks(PnPContext sourceContext, PnPContext targetContext)
            {
                throw new NotImplementedException();
            }

            Task ITransformationStateManager.WriteStateAsync<T>(string name, T state)
            {
                throw new NotImplementedException();
            }

            Task<T> ITransformationStateManager.ReadStateAsync<T>(string name)
            {
                throw new NotImplementedException();
            }

            Action<TransformationExecutionStatus> ITransformationExecutor.Progress { get; set; }

            Task<TransformationExecutionStatus> ITransformationExecutor.GetStatusAsync(Guid processId)
            {
                throw new NotImplementedException();
            }

            Task<Guid> ITransformationExecutor.StartTransformAsync(PnPContext sourceContext, PnPContext targetContext)
            {
                throw new NotImplementedException();
            }

            Task ITransformationExecutor.StopTransformAsync(Guid processId)
            {
                throw new NotImplementedException();
            }
        }

        #endregion 
    }
}
