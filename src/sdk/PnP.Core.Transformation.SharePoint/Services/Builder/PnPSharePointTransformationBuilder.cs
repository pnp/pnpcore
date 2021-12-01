using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Transformation.Services.Builder;
using PnP.Core.Transformation.SharePoint.Services.Builder.Configuration;
using System;

namespace PnP.Core.Transformation.SharePoint.Services.Builder
{
    /// <summary>
    /// Used to configure PnP Core Transformation for SharePoint
    /// </summary>
    public class PnPSharePointTransformationBuilder : PnPTransformationBuilder, IPnPSharePointTransformationBuilder
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="services">The services being configured</param>
        public PnPSharePointTransformationBuilder(IServiceCollection services) : base(services)
        {}

        /// <summary>
        /// Customizes the default transformation options for SharePoint
        /// </summary>
        /// <param name="options">The configuration options</param>
        /// <returns>The builder object for the current configuration</returns>
        public IPnPSharePointTransformationBuilder WithSharePointOptions(Action<SharePointTransformationOptions> options)
        {
            Services.Configure(options);
            return this;
        }
    }
}
