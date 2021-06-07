using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Test.Utilities;

namespace PnP.Core.Transformation.SharePoint.Test
{
    [TestClass]
    public class TransformationTests
    {

        [TestMethod]
        public async Task SharepointTransformAsync()
        {
            var services = new ServiceCollection();
            services.AddTestPnPCore();
            services.AddPnPSharePointTransformation();

            var provider = services.BuildServiceProvider();

            var pnpContextFactory = provider.GetRequiredService<IPnPContextFactory>();
            var pageTransformator = provider.GetRequiredService<IPageTransformator>();

            var sourceContext = await pnpContextFactory.CreateAsync(TestCommon.SourceTestSite);
            var targetContext = await pnpContextFactory.CreateAsync(TestCommon.TargetTestSite);
            var sourceUri = new Uri("http://site/item");

            var result = await pageTransformator.TransformSharePointAsync(sourceContext, targetContext, sourceUri);

            Assert.IsNotNull(result);
            Assert.AreEqual(new Uri("https://officedevpnp.sharepoint.com/sites/pnpcoresdkdemo/item"), result);
        }

        [TestMethod]
        public async Task InMemoryExecutorSharePointTransformAsync()
        {
            var services = new ServiceCollection();
            services.AddTestPnPCore();
            services.AddPnPSharePointTransformation();

            var provider = services.BuildServiceProvider();

            var transformationExecutor = provider.GetRequiredService<ITransformationExecutor>();
            var pnpContextFactory = provider.GetRequiredService<IPnPContextFactory>();

            var result = await transformationExecutor.TransformSharePointAsync(
                pnpContextFactory,
                TestCommon.SourceTestSite,
                TestCommon.TargetTestSite);

            Assert.IsNotNull(result);
            Assert.AreEqual(TransformationExecutionState.Completed, result.State);
        }

    }
}
