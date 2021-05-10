using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PnP.Core.Modernization.Services.Core;
using PnP.Core.Modernization.Services.MappingProviders;

namespace PnP.Core.Modernization.Services.Builder
{
    /// <summary>
    /// Used to configure PnP Core Modernization
    /// </summary>
    public class PnPModernizationBuilder : IPnPModernizationBuilder
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="services">The services being configured.</param>
        public PnPModernizationBuilder(IServiceCollection services) => Services = services;

        /// <summary>
        /// The services being configured
        /// </summary>
        public virtual IServiceCollection Services { get; }

        /// <summary>
        /// Set a custom <see cref="ITransformationStateManager" /> to use to handle the state of a transformation process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPModernizationBuilder WithTransformationStateManager<T>()
            where T : class, ITransformationStateManager
        {
            Services.RemoveAll<ITransformationStateManager>();
            Services.AddTransient<ITransformationStateManager, T>();
            return this;
        }

        /// <summary>
        /// Set a custom <see cref="ITransformationDistiller" /> to defines a list of pages to transform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPModernizationBuilder WithTransformationDistiller<T>()
            where T : class, ITransformationDistiller
        {
            Services.RemoveAll<ITransformationDistiller>();
            Services.AddTransient<ITransformationDistiller, T>();
            return this;
        }

        /// <summary>
        /// Set a custom <see cref="IPageTransformator" /> to use to transform pages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPModernizationBuilder WithPageTransformator<T>()
            where T : class, IPageTransformator
        {
            Services.RemoveAll<IPageTransformator>();
            Services.AddTransient<IPageTransformator, T>();
            return this;
        }

        /// <summary>
        /// Set a custom <see cref="ITransformationExecutor" /> that manages the transformation of one or more pages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPModernizationBuilder WithTransformationExecutor<T>()
            where T : class, ITransformationExecutor
        {
            Services.RemoveAll<ITransformationExecutor>();
            Services.AddTransient<ITransformationExecutor, T>();
            return this;
        }

        /// <summary>
        /// Customize the default transformation options for the pages
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IPnPModernizationBuilder WithPageOptions(Action<PageTransformationOptions> options)
        {
            Services.Configure(options);
            return this;
        }

        /// <summary>
        /// Set a custom <see cref="IWebPartMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPModernizationBuilder WithWebPartMappingProvider<T>()
            where T : class, IWebPartMappingProvider
        {
            return WithProvider<T>((o, t) => o.WebPartMappingProvider = t);
        }

        /// <summary>
        /// Set a custom <see cref="IPageLayoutMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPModernizationBuilder WithPageLayoutMappingProvider<T>() where T : class, IPageLayoutMappingProvider
        {
            return WithProvider<T>((o, t) => o.PageLayoutMappingProvider = t);
        }

        /// <summary>
        /// Set a custom <see cref="ITaxonomyMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPModernizationBuilder WithTaxonomyMappingProvider<T>()
            where T : class, ITaxonomyMappingProvider
        {
            return WithProvider<T>((o, t) => o.TaxonomyMappingProvider = t);
        }

        /// <summary>
        /// Set a custom <see cref="IMetadataMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPModernizationBuilder WithMetadataMappingProvider<T>()
            where T : class, IMetadataMappingProvider
        {
            return WithProvider<T>((o, t) => o.MetadataMappingProvider = t);
        }

        /// <summary>
        /// Set a custom <see cref="IUrlMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPModernizationBuilder WithUrlMappingProvider<T>()
            where T : class, IUrlMappingProvider
        {
            return WithProvider<T>((o, t) => o.UrlMappingProvider = t);
        }

        /// <summary>
        /// Set a custom <see cref="IUserMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPModernizationBuilder WithUserMappingProvider<T>()
            where T : class, IUserMappingProvider
        {
            return WithProvider<T>((o, t) => o.UserMappingProvider = t);
        }

        private IPnPModernizationBuilder WithProvider<T>(Action<PageTransformationOptions, T> setAction)
            where T : class
        {
            Services.RemoveAll<T>();
            Services.AddTransient<T>();
            Services.AddOptions<PageTransformationOptions>().Configure(setAction);

            return this;
        }

    }
}
