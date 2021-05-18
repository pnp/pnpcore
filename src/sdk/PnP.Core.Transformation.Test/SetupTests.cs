using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Services.MappingProviders;
using PnP.Core.Transformation.Services.Builder.Configuration;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Services;

namespace PnP.Core.Transformation.Test
{
    [TestClass]
    public class SetupTests
    {

        [TestMethod]
        public void DefaultServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddPnPTransformation()
                .WithTransformationDistiller<Mock>();

            var provider = services.BuildServiceProvider();

            // TODO: check all types

            Assert.IsInstanceOfType(provider.GetRequiredService<IPageTransformator>(), typeof(DefaultPageTransformator));
            Assert.IsInstanceOfType(provider.GetRequiredService<IMappingProvider>(), typeof(DefaultMappingProvider));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITransformationStateManager>(), typeof(InMemoryTransformationStateManager));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITransformationExecutor>(), typeof(InProcessTransformationExecutor));
        }

        [TestMethod]
        public void CustomServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddPnPTransformation(o => o.DisableTelemetry = true)
                .WithPageOptions(o => o.DisablePageComments = true)

                .WithMappingProvider<Mock>()
                .WithMetadataMappingProvider<Mock>()
                .WithPageLayoutMappingProvider<Mock>()
                .WithTaxonomyMappingProvider<Mock>()
                .WithUrlMappingProvider<Mock>()
                .WithUserMappingProvider<Mock>()
                .WithWebPartMappingProvider<Mock>()
                .WithHtmlMappingProvider<Mock>()

                .WithPageTransformator<Mock>()
                .AddPagePostTransformation<Mock>()
                .AddPagePostTransformation<Mock>()
                .AddPagePreTransformation<Mock>()
                .AddPagePreTransformation<Mock>()

                .WithTransformationDistiller<Mock>()
                .WithTransformationStateManager<Mock>()
                .WithTransformationExecutor<Mock>();

            var provider = services.BuildServiceProvider();

            var pnpModernizationOptions = provider.GetRequiredService<IOptions<PnPTransformationOptions>>().Value;
            Assert.IsTrue(pnpModernizationOptions.DisableTelemetry);

            var transformationOptions = provider.GetRequiredService<IOptions<PageTransformationOptions>>().Value;
            Assert.IsTrue(transformationOptions.DisablePageComments);

            Assert.IsInstanceOfType(transformationOptions.MetadataMappingProvider, typeof(Mock));
            Assert.IsInstanceOfType(transformationOptions.PageLayoutMappingProvider, typeof(Mock));
            Assert.IsInstanceOfType(transformationOptions.TaxonomyMappingProvider, typeof(Mock));
            Assert.IsInstanceOfType(transformationOptions.UrlMappingProvider, typeof(Mock));
            Assert.IsInstanceOfType(transformationOptions.UserMappingProvider, typeof(Mock));
            Assert.IsInstanceOfType(transformationOptions.MetadataMappingProvider, typeof(Mock));
            Assert.IsInstanceOfType(transformationOptions.HtmlMappingProvider, typeof(Mock));

            Assert.IsInstanceOfType(provider.GetRequiredService<IMappingProvider>(), typeof(Mock));
            Assert.IsInstanceOfType(provider.GetRequiredService<IPageTransformator>(), typeof(Mock));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITransformationDistiller>(), typeof(Mock));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITransformationStateManager>(), typeof(Mock));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITransformationExecutor>(), typeof(Mock));

            Assert.AreEqual(2, provider.GetServices<IPagePreTransformation>().Count());
            Assert.AreEqual(2, provider.GetServices<IPagePostTransformation>().Count());
            Assert.IsInstanceOfType(provider.GetRequiredService<IPagePostTransformation>(), typeof(Mock));
            Assert.IsInstanceOfType(provider.GetRequiredService<IPagePreTransformation>(), typeof(Mock));
        }

        #region Mock

        private class Mock :
            IMappingProvider,
            IWebPartMappingProvider,
            IUserMappingProvider,
            ITaxonomyMappingProvider,
            IPageLayoutMappingProvider,
            IMetadataMappingProvider,
            IUrlMappingProvider,
            IPageTransformator,
            ITransformationDistiller,
            ITransformationStateManager,
            ITransformationExecutor,
            IHtmlMappingProvider,
            IPagePreTransformation,
            IPagePostTransformation
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

            IAsyncEnumerable<PageTransformationTask> ITransformationDistiller.GetTransformationTasks(PnPContext sourceContext, PnPContext targetContext)
            {
                throw new NotImplementedException();
            }

            Task<TransformationProcess> ITransformationExecutor.CreateTransformationProcessAsync(PnPContext sourceContext, PnPContext targetContext)
            {
                throw new NotImplementedException();
            }

            Task<TransformationProcess> ITransformationExecutor.LoadTransformationProcessAsync(Guid processId)
            {
                throw new NotImplementedException();
            }

            Task ITransformationStateManager.WriteStateAsync<T>(object key, T state)
            {
                throw new NotImplementedException();
            }

            Task<T> ITransformationStateManager.ReadStateAsync<T>(object key)
            {
                throw new NotImplementedException();
            }

            public Task<HtmlMappingProviderOutput> MapHtmlAsync(HtmlMappingProviderInput input)
            {
                throw new NotImplementedException();
            }

            public Task<MappingProviderOutput> MapAsync(MappingProviderInput input)
            {
                throw new NotImplementedException();
            }

            public Task PreTransformAsync(PagePreTransformationContext context)
            {
                throw new NotImplementedException();
            }

            public Task PostTransformAsync(PagePostTransformationContext context)
            {
                throw new NotImplementedException();
            }
        }

        #endregion 
    }
}
