using PnP.Core.Transformation.Services.Builder;
using PnP.Core.Transformation.SharePoint.Services.Builder.Configuration;
using System;

namespace PnP.Core.Transformation.SharePoint.Services.Builder
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
