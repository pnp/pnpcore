using System;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Transformation.Services.Builder;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Builder.Configuration;

namespace PnP.Core.Transformation.SharePoint.Builder
{
    /// <summary>
    /// Used to configure PnP Core Transformation for SharePoint
    /// </summary>
    public interface IPnPSharePointTransformationBuilder : IPnPTransformationBuilder
    {
        /// <summary>
        /// Customizes the default transformation options for SharePoint
        /// </summary>
        /// <param name="options">The configuration options</param>
        /// <returns>The builder object for the current configuration</returns>
        IPnPSharePointTransformationBuilder WithSharePointOptions(Action<SharePointTransformationOptions> options);

    }
}
