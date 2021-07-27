using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SharePoint.Client;
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
            // services.AddPnPSharePointTransformation();
            services.AddPnPSharePointTransformation(null, spOptions =>
            {
                //spOptions.WebPartMappingFile = @"C:\github\pnpcore\src\sdk\PnP.Core.Transformation.SharePoint\MappingFiles\webpartmapping.xml";
                //spOptions.PageLayoutMappingFile = @"C:\github\pnpcore\src\sdk\PnP.Core.Transformation.SharePoint\MappingFiles\pagelayoutmapping.xml";
                spOptions.CopyPageMetadata = true;
                spOptions.KeepPageSpecificPermissions = true;
                spOptions.RemoveEmptySectionsAndColumns = true;
                spOptions.ShouldMapUsers = true;
                spOptions.TargetPageTakesSourcePageName = true;
            });

            var provider = services.BuildServiceProvider();

            var pnpContextFactory = provider.GetRequiredService<IPnPContextFactory>();
            var pageTransformator = provider.GetRequiredService<IPageTransformator>();

            var sourceContext = provider.GetRequiredService<ClientContext>();
            var targetContext = await pnpContextFactory.CreateAsync(TestCommon.TargetTestSite);
            var sourceUri = new Uri("https://piasysdev.sharepoint.com/sites/ClassicTest01/SitePages/tourdefrance2018.aspx");
            // var sourceUri = new Uri("https://piasysdev.sharepoint.com/sites/ClassicTest01/SitePages/ToMigrate_giro2018.aspx");
            
            var result = await pageTransformator.TransformSharePointAsync(sourceContext, targetContext, sourceUri);

            Assert.IsNotNull(result);
            var expectedUri = new Uri($"{targetContext.Web.Url}/SitePages/Migrated_{sourceUri.Segments[sourceUri.Segments.Length - 1]}");
            Assert.AreEqual(expectedUri, result);
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

            var sourceContext = provider.GetRequiredService<ClientContext>();

            var result = await transformationExecutor.TransformSharePointAsync(
                pnpContextFactory,
                sourceContext,
                TestCommon.TargetTestSite);

            Assert.IsNotNull(result);
            Assert.AreEqual(TransformationExecutionState.Completed, result.State);
        }

    }
}
