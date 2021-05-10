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
