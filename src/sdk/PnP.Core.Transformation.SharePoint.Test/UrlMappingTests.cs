using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.MappingProviders;
using PnP.Core.Transformation.Test.Utilities;

namespace PnP.Core.Transformation.SharePoint.Test
{
    [TestClass]
    public class UrlMappingTests
    {
        [TestMethod]
        public async Task MapUrlAsync()
        {
            var services = new ServiceCollection();
            services.AddTestPnPCore();
            services.AddPnPSharePointTransformation();

            var provider = services.BuildServiceProvider();

            var pnpContextFactory = provider.GetRequiredService<IPnPContextFactory>();
            var mappingProvider = provider.GetRequiredService<IUrlMappingProvider>();

            // Prepare contexts
            var sourceContext = new ClientContext("https://capadevtest.sharepoint.com/sites/PnPSauce");
            var targetContext = await pnpContextFactory.CreateAsync(TestCommon.TargetTestSite);

            var sourceUri = new Uri("https://capadevtest.sharepoint.com/Documents/Folder/Employee-Handbook.docx");
            var targetPageUri = new Uri("http://site/item");

            var sourceItem = new SharePointSourceItem(sourceUri, sourceContext);

            // Prepare task
            var task = new PageTransformationTask(new SharePointSourceProvider(sourceContext), sourceItem.Id, targetContext);
            var context = new PageTransformationContext(task, sourceItem, targetPageUri);

            // Map url
            var input = new UrlMappingProviderInput(context, sourceUri.ToString());
            var result = await mappingProvider.MapUrlAsync(input);

            Assert.AreEqual("https://capadevtest.sharepoint.com/sites/PnPSauceModern/Documents/Folder/Employee-Handbook.docx", result.Text);
        }
    }
}
