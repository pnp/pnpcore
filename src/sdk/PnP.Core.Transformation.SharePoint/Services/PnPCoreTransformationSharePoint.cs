using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using PnP.Core.Transformation.Services.Builder.Configuration;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.SharePoint.Services.Builder.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PnP.Core.Transformation.SharePoint.Services
{
    /// <summary>
    /// Class that implements the inner logic to activate a PnP Transformation, hiding the Dependency Injection logic
    /// </summary>
    public class PnPCoreTransformationSharePoint
    {
        private ServiceProvider serviceProvider;

        /// <summary>
        /// Constructor with configuration options
        /// </summary>
        public PnPCoreTransformationSharePoint(
            Action<PnPTransformationOptions> pnpOptions,
            Action<PageTransformationOptions> pageOptions,
            Action<SharePointTransformationOptions> sharePointOptions)
        {
            InitServices(pnpOptions, pageOptions, sharePointOptions);
        }

        private void InitServices(
            Action<PnPTransformationOptions> pnpOptions,
            Action<PageTransformationOptions> pageOptions,
            Action<SharePointTransformationOptions> sharePointOptions)
        {
            // Build the service collection and load PnP Core SDK
            IServiceCollection services = new ServiceCollection();

            services.AddLogging(builder => {
                // builder.AddConsole();
                var b = builder;
            });

            // To increase coverage of solutions providing tokens without graph scopes we turn of graphfirst for PnPContext created from PnP Framework                
            services = services.AddPnPCore(options =>
            {
                options.PnPContext.GraphFirst = false;
            }).Services;

            // Use the default settings for PnP Transformation Framework in SharePoint
            services.AddPnPSharePointTransformation(
                pnpOptions, pageOptions, sharePointOptions);

            this.serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Provides a PnP Page Transformator instance
        /// </summary>
        /// <returns>The Page Transformator instance</returns>
        public IPageTransformator GetPnPSharePointPageTransformator()
        {
            // Get the configured page transformator
            var pageTransformator = serviceProvider.GetRequiredService<IPageTransformator>();
            return pageTransformator;
        }
    }
}
