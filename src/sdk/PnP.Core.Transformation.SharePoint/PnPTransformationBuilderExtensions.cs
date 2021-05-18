using PnP.Core.Transformation.Services;
using PnP.Core.Transformation.Services.Builder;
using PnP.Core.Transformation.Services.Builder.Configuration;
using System;
using System.Reflection.Emit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint;
using PnP.Core.Transformation.SharePoint.MappingProviders;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up PnP Transformation services in an <see cref="IPnPTransformationBuilder" />.
    /// </summary>
    public static class PnPTransformationBuilderExtensions
    {
        /// <summary>
        /// Adds default implementations provided by the transformation framework
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IPnPTransformationBuilder WithSharePointMappings(this IPnPTransformationBuilder builder)
        {
            return WithSharePointMappings(builder, null);
        }

        /// <summary>
        /// Adds default implementations provided by the transformation framework
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IPnPTransformationBuilder WithSharePointMappings(this IPnPTransformationBuilder builder, Action<SharePointTransformationOptions> options)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            if (options != null)
            {
                builder.Services.Configure(options);
            }

            builder.WithMappingProvider<SharePointMappingProvider>()
                .WithSourceProvider<SharePointSourceProvider>();

            builder.Services.AddTransient<IMetadataMappingProvider, SharePointMetadataMappingProvider>()
                .AddTransient<IHtmlMappingProvider, SharePointHtmlMappingProvider>()
                .AddTransient<IPageLayoutMappingProvider, SharePointPageLayoutMappingProvider>()
                .AddTransient<ITaxonomyMappingProvider, SharePointTaxonomyMappingProvider>()
                .AddTransient<IUserMappingProvider, SharePointUserMappingProvider>()
                .AddTransient<IUrlMappingProvider, SharePointUrlMappingProvider>()
                .AddTransient<IWebPartMappingProvider, SharePointWebPartMappingProvider>();

            return builder;
        }

        /// <summary>
        /// Customizes the default transformation options for the pages
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IPnPTransformationBuilder WithSharePointMappingOptions(this IPnPTransformationBuilder builder, Action<PageTransformationOptions> options)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.Configure(options);
            return builder;
        }
    }
}
