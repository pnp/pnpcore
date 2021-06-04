using PnP.Core.Transformation.Services;
using PnP.Core.Transformation.Services.Builder;
using PnP.Core.Transformation.Services.Builder.Configuration;
using System;
using System.Reflection.Emit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint;
using PnP.Core.Transformation.SharePoint.Builder;
using PnP.Core.Transformation.SharePoint.Builder.Configuration;
using PnP.Core.Transformation.SharePoint.MappingProviders;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up PnP Transformation services in an <see cref="IPnPTransformationBuilder" />.
    /// </summary>
    public static class PnPSharePointTransformationBuilderExtensions
    {
        /// <summary>
        /// Configures PnP Transformation for SharePoint with default options
        /// </summary>
        /// <param name="services">The collection of services in an <see cref="IServiceCollection" /></param>
        /// <returns>A PnPTransformationBuilder instance</returns>
        public static IPnPSharePointTransformationBuilder AddPnPSharePointTransformation(this IServiceCollection services)
        {
            return AddPnPSharePointTransformation(services, null);
        }

        /// <summary>
        /// Configures PnP Transformation for SharePoint with default options
        /// </summary>
        /// <param name="services">The collection of services in an <see cref="IServiceCollection" /></param>
        /// <returns>A PnPTransformationBuilder instance</returns>
        public static IPnPSharePointTransformationBuilder AddPnPSharePointTransformation(this IServiceCollection services, Action<PnPTransformationOptions> options)
        {
            return services.AddPnPTransformation(options)
                .WithSharePoint();
        }

        /// <summary>
        /// Adds default implementations provided by the transformation framework
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IPnPSharePointTransformationBuilder WithSharePoint(this IPnPTransformationBuilder builder)
        {
            return WithSharePoint(builder, null);
        }

        /// <summary>
        /// Adds default implementations provided by the transformation framework
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IPnPSharePointTransformationBuilder WithSharePoint(this IPnPTransformationBuilder builder, Action<SharePointTransformationOptions> options)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            if (options != null)
            {
                builder.Services.Configure(options);
            }

            builder.WithMappingProvider<SharePointMappingProvider>()
                .WithTransformationDistiller<SharePointTransformationDistiller>()
                .WithTargetPageUriResolver<SharePointTargetPageUriResolver>();

            builder.Services.TryAddTransient<IMetadataMappingProvider, SharePointMetadataMappingProvider>();
            builder.Services.TryAddTransient<IHtmlMappingProvider, SharePointHtmlMappingProvider>();
            builder.Services.TryAddTransient<IPageLayoutMappingProvider, SharePointPageLayoutMappingProvider>();
            builder.Services.TryAddTransient<ITaxonomyMappingProvider, SharePointTaxonomyMappingProvider>();
            builder.Services.TryAddTransient<IUserMappingProvider, SharePointUserMappingProvider>();
            builder.Services.TryAddTransient<IUrlMappingProvider, SharePointUrlMappingProvider>();
            builder.Services.TryAddTransient<IWebPartMappingProvider, SharePointWebPartMappingProvider>();

            return new PnPSharePointTransformationBuilder(builder.Services);
        }
    }
}
