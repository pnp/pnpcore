using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PnP.Core.Transformation.Services.Builder;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Builder.Configuration;

namespace PnP.Core.Transformation.SharePoint.Builder
{
    /// <summary>
    /// Used to configure PnP Core Transformation for SharePoint
    /// </summary>
    public class PnPSharePointTransformationBuilder : PnPTransformationBuilder, IPnPSharePointTransformationBuilder
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="services">The services being configured.</param>
        public PnPSharePointTransformationBuilder(IServiceCollection services) : base(services)
        {}

        /// <summary>
        /// Customizes the default transformation options for SharePoint
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IPnPSharePointTransformationBuilder WithSharePointOptions(Action<SharePointTransformationOptions> options)
        {
            Services.Configure(options);
            return this;
        }
    }
}
