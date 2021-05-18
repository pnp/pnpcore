using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Transformation.Services.Builder.Configuration;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Services;
using PnP.Core.Transformation.Services.Builder;

namespace PnP.Core.Transformation.Test
{
    [TestClass]
    public class TransformationTests
    {

        [TestMethod]
        public async Task TransformAsync()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddPnPTransformation();

            var provider = services.BuildServiceProvider();

            // TODO: complete
            ISourceItem sourceItem = null;
            PnPContext targetContext = null;
            Uri targetPageUri = null;

            var pageTransformator = provider.GetRequiredService<IPageTransformator>();
            await pageTransformator.TransformAsync(new PageTransformationTask(sourceItem, targetContext, targetPageUri));
        }

        //[TestMethod]
        //public async Task SharepointTransformAsync()
        //{
        //    var services = new ServiceCollection();
        //    services.AddLogging();
        //    services.AddPnPTransformation()
        //        .WithSharePointMappings();

        //    var provider = services.BuildServiceProvider();

        //    var pageTransformator = provider.GetRequiredService<IPageTransformator>();
        //    await pageTransformator.TransformAsync(new PageTransformationTask());
        //}

    }
}
