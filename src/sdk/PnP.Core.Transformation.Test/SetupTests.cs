using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Transformation.Services.Builder.Configuration;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Services;
using PnP.Core.Transformation.SharePoint;
using PnP.Core.Transformation.SharePoint.MappingProviders;

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
                .WithMappingProvider<Mock>();

            var provider = services.BuildServiceProvider();

            // TODO: check all types

            Assert.IsInstanceOfType(provider.GetRequiredService<IPageTransformator>(), typeof(DefaultPageTransformator));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITransformationStateManager>(), typeof(InMemoryTransformationStateManager));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITransformationExecutor>(), typeof(InProcessTransformationExecutor));
        }

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

            Assert.IsInstanceOfType(provider.GetRequiredService<IMetadataMappingProvider>(), typeof(SharePointMetadataMappingProvider));
            Assert.IsInstanceOfType(provider.GetRequiredService<IHtmlMappingProvider>(), typeof(SharePointHtmlMappingProvider));
            Assert.IsInstanceOfType(provider.GetRequiredService<IPageLayoutMappingProvider>(), typeof(SharePointPageLayoutMappingProvider));
            Assert.IsInstanceOfType(provider.GetRequiredService<ITaxonomyMappingProvider>(), typeof(SharePointTaxonomyMappingProvider));
            Assert.IsInstanceOfType(provider.GetRequiredService<IUserMappingProvider>(), typeof(SharePointUserMappingProvider));
            Assert.IsInstanceOfType(provider.GetRequiredService<IUrlMappingProvider>(), typeof(SharePointUrlMappingProvider));
            Assert.IsInstanceOfType(provider.GetRequiredService<IWebPartMappingProvider>(), typeof(SharePointWebPartMappingProvider));
        }

        [TestMethod]
        public void CustomServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            services.AddPnPTransformation(o => o.DisableTelemetry = true)
                .WithPageOptions(o => o.DisablePageComments = true)

                .WithMappingProvider<Mock>()

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
            IPageTransformator,
            ITransformationDistiller,
            ITransformationStateManager,
            ITransformationExecutor,
            IHtmlMappingProvider,
            IPagePreTransformation,
            IPagePostTransformation
        {
            Task<MappingProviderOutput> IMappingProvider.MapAsync(MappingProviderInput input, CancellationToken token)
            {
                throw new NotImplementedException();
            }

            Task<Uri> IPageTransformator.TransformAsync(PageTransformationTask task, CancellationToken token)
            {
                throw new NotImplementedException();
            }

            IAsyncEnumerable<PageTransformationTask> ITransformationDistiller.GetPageTransformationTasksAsync(ISourceProvider sourceProvider, PnPContext targetContext,
                CancellationToken token)
            {
                throw new NotImplementedException();
            }

            Task ITransformationStateManager.WriteStateAsync<T>(object key, T state, CancellationToken token)
            {
                throw new NotImplementedException();
            }

            Task<T> ITransformationStateManager.ReadStateAsync<T>(object key, CancellationToken token)
            {
                throw new NotImplementedException();
            }

            Task<ITransformationProcess> ITransformationExecutor.CreateTransformationProcessAsync(ISourceProvider sourceProvider, PnPContext targetContext,
                CancellationToken token)
            {
                throw new NotImplementedException();
            }

            Task<ITransformationProcess> ITransformationExecutor.LoadTransformationProcessAsync(Guid processId, CancellationToken token)
            {
                throw new NotImplementedException();
            }

            Task<HtmlMappingProviderOutput> IHtmlMappingProvider.MapHtmlAsync(HtmlMappingProviderInput input, CancellationToken token)
            {
                throw new NotImplementedException();
            }

            Task IPagePreTransformation.PreTransformAsync(PagePreTransformationContext context, CancellationToken token)
            {
                throw new NotImplementedException();
            }

            Task IPagePostTransformation.PostTransformAsync(PagePostTransformationContext context, CancellationToken token)
            {
                throw new NotImplementedException();
            }
        }

        #endregion 
    }
}
