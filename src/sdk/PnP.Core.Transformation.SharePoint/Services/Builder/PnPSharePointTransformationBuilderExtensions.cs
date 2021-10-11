using PnP.Core.Transformation.Services;
using PnP.Core.Transformation.Services.Builder;
using PnP.Core.Transformation.Services.Builder.Configuration;
using System;
using System.Reflection.Emit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint;
using PnP.Core.Transformation.SharePoint.Services.Builder;
using PnP.Core.Transformation.SharePoint.Services.Builder.Configuration;
using PnP.Core.Transformation.SharePoint.MappingProviders;
using PnP.Core.Transformation.SharePoint.Publishing;
using PnP.Core.Transformation.SharePoint.Functions;
using PnP.Core.Transformation.SharePoint.Services;

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
            return AddPnPSharePointTransformation(services, null, null);
        }

        /// <summary>
        /// Configures PnP Transformation for SharePoint with default options
        /// </summary>
        /// <param name="services">The collection of services in an <see cref="IServiceCollection" /></param>
        /// <param name="pnpOptions">An Action to configure the PnP Transformation global options</param>
        /// <param name="pageOptions">An Action to configure the page transformation options</param>
        /// <returns>A PnPTransformationBuilder instance</returns>
        public static IPnPSharePointTransformationBuilder AddPnPSharePointTransformation(this IServiceCollection services, 
            Action<PnPTransformationOptions> pnpOptions,
            Action<PageTransformationOptions> pageOptions)
        {
            return services.AddPnPTransformation(pnpOptions, pageOptions)
                .WithSharePoint();
        }

        /// <summary>
        /// Configures PnP Transformation for SharePoint with default options
        /// </summary>
        /// <param name="services">The collection of services in an <see cref="IServiceCollection" /></param>
        /// <param name="pnpOptions">An Action to configure the PnP Transformation global options</param>
        /// <param name="pageOptions">An Action to configure the page transformation options</param>
        /// <param name="sharePointOptions">An Action to configure the SharePoint transformation options</param>
        /// <returns>A PnPTransformationBuilder instance</returns>
        public static IPnPSharePointTransformationBuilder AddPnPSharePointTransformation(this IServiceCollection services, 
            Action<PnPTransformationOptions> pnpOptions,
            Action<PageTransformationOptions> pageOptions,
            Action<SharePointTransformationOptions> sharePointOptions)
        {
            return services.AddPnPTransformation(pnpOptions, pageOptions)
                .WithSharePoint(sharePointOptions);
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

            // Add the custom PageLayoutAnalyser type
            builder.Services.TryAddTransient<PageLayoutAnalyser, PageLayoutAnalyser>();

            // Add the SharePoint functions services
            builder.Services.TryAddTransient<FunctionProcessor, FunctionProcessor>();
            builder.Services.TryAddTransient<SharePointFunctionsService, SharePointFunctionsService>();
            builder.Services.TryAddTransient<PublishingFunctionProcessor, PublishingFunctionProcessor>();
            builder.Services.TryAddTransient<SharePointPublishingFunctionsService, SharePointPublishingFunctionsService>();

            // Add the HTML Transformator service
            builder.Services.TryAddTransient<HtmlTransformator, HtmlTransformator>();

            // Add the Wiki HTML Transformator service
            builder.Services.TryAddTransient<WikiHtmlTransformator, WikiHtmlTransformator>();

            // Add the Publishing Page Transformator service
            builder.Services.TryAddTransient<PublishingLayoutTransformator, PublishingLayoutTransformator>();            

            return new PnPSharePointTransformationBuilder(builder.Services);
        }
    }
}
