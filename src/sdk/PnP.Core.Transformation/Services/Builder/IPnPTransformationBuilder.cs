using System;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;

namespace PnP.Core.Transformation.Services.Builder
{
    /// <summary>
    /// Used to configure PnP Core Transformation
    /// </summary>
    public interface IPnPTransformationBuilder
    {
        /// <summary>
        /// Collection of services for Dependecy Injection
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Set a custom <see cref="ITransformationStateManager" /> to use to handle the state of a transformation process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithTransformationStateManager<T>()
            where T : class, ITransformationStateManager;

        /// <summary>
        /// Set a custom <see cref="ITransformationDistiller" /> to defines a list of pages to transform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithTransformationDistiller<T>()
            where T : class, ITransformationDistiller;

        /// <summary>
        /// Set a custom <see cref="IPageTransformator" /> to use to transform pages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithPageTransformator<T>()
            where T : class, IPageTransformator;

        /// <summary>
        /// Set a custom <see cref="ITransformationExecutor" /> that manages the transformation of one or more pages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithTransformationExecutor<T>()
            where T : class, ITransformationExecutor;

        /// <summary>
        /// Customize the default transformation options for the pages
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        IPnPTransformationBuilder WithPageOptions(Action<PageTransformationOptions> options);

        /// <summary>
        /// Set a custom <see cref="IWebPartMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithWebPartMappingProvider<T>()
            where T : class, IWebPartMappingProvider;

        /// <summary>
        /// Set a custom <see cref="IPageLayoutMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithPageLayoutMappingProvider<T>()
            where T : class, IPageLayoutMappingProvider;

        /// <summary>
        /// Set a custom <see cref="ITaxonomyMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithTaxonomyMappingProvider<T>()
            where T : class, ITaxonomyMappingProvider;

        /// <summary>
        /// Set a custom <see cref="IMetadataMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithMetadataMappingProvider<T>()
            where T : class, IMetadataMappingProvider;

        /// <summary>
        /// Set a custom <see cref="IUrlMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithUrlMappingProvider<T>()
            where T : class, IUrlMappingProvider;

        /// <summary>
        /// Set a custom <see cref="IUserMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithUserMappingProvider<T>()
            where T : class, IUserMappingProvider;
    }
}
