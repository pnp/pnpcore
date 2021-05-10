using System;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Modernization.Services.Core;
using PnP.Core.Modernization.Services.MappingProviders;

namespace PnP.Core.Modernization.Services.Builder
{
    /// <summary>
    /// Used to configure PnP Core Modernization
    /// </summary>
    public interface IPnPModernizationBuilder
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
        IPnPModernizationBuilder WithTransformationStateManager<T>()
            where T : class, ITransformationStateManager;

        /// <summary>
        /// Set a custom <see cref="ITransformationDistiller" /> to defines a list of pages to transform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPModernizationBuilder WithTransformationDistiller<T>()
            where T : class, ITransformationDistiller;

        /// <summary>
        /// Set a custom <see cref="IPageTransformator" /> to use to transform pages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPModernizationBuilder WithPageTransformator<T>()
            where T : class, IPageTransformator;

        /// <summary>
        /// Set a custom <see cref="ITransformationExecutor" /> that manages the transformation of one or more pages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPModernizationBuilder WithTransformationExecutor<T>()
            where T : class, ITransformationExecutor;

        /// <summary>
        /// Customize the default transformation options for the pages
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        IPnPModernizationBuilder WithPageOptions(Action<PageTransformationOptions> options);

        /// <summary>
        /// Set a custom <see cref="IWebPartMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPModernizationBuilder WithWebPartMappingProvider<T>()
            where T : class, IWebPartMappingProvider;

        /// <summary>
        /// Set a custom <see cref="IPageLayoutMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPModernizationBuilder WithPageLayoutMappingProvider<T>()
            where T : class, IPageLayoutMappingProvider;

        /// <summary>
        /// Set a custom <see cref="ITaxonomyMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPModernizationBuilder WithTaxonomyMappingProvider<T>()
            where T : class, ITaxonomyMappingProvider;

        /// <summary>
        /// Set a custom <see cref="IMetadataMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPModernizationBuilder WithMetadataMappingProvider<T>()
            where T : class, IMetadataMappingProvider;

        /// <summary>
        /// Set a custom <see cref="IUrlMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPModernizationBuilder WithUrlMappingProvider<T>()
            where T : class, IUrlMappingProvider;

        /// <summary>
        /// Set a custom <see cref="IUserMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPModernizationBuilder WithUserMappingProvider<T>()
            where T : class, IUserMappingProvider;
    }
}
