using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using System;

namespace PnP.Core.Transformation.Services.Builder
{
    /// <summary>
    /// Used to configure PnP Core Transformation
    /// </summary>
    public class PnPTransformationBuilder : IPnPTransformationBuilder
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="services">The services being configured.</param>
        public PnPTransformationBuilder(IServiceCollection services) => Services = services;

        /// <summary>
        /// The services being configured
        /// </summary>
        public virtual IServiceCollection Services { get; }

        /// <summary>
        /// Allows configuring a custom <see cref="ITransformationStateManager" /> to use to handle the state of a transformation process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithTransformationStateManager<T>()
            where T : class, ITransformationStateManager
        {
            Services.RemoveAll<ITransformationStateManager>();
            Services.AddTransient<ITransformationStateManager, T>();
            return this;
        }

        /// <summary>
        /// Allows configuring a custom <see cref="ITransformationDistiller" /> to defines a list of pages to transform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithTransformationDistiller<T>()
            where T : class, ITransformationDistiller
        {
            Services.RemoveAll<ITransformationDistiller>();
            Services.AddTransient<ITransformationDistiller, T>();
            return this;
        }

        /// <summary>
        /// Allows configuring a custom <see cref="IPageTransformator" /> to use to transform pages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithPageTransformator<T>()
            where T : class, IPageTransformator
        {
            Services.RemoveAll<IPageTransformator>();
            Services.AddTransient<IPageTransformator, T>();
            return this;
        }

        /// <summary>
        /// Allows configuring a custom <see cref="ITransformationExecutor" /> that manages the transformation of one or more pages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithTransformationExecutor<T>()
            where T : class, ITransformationExecutor
        {
            Services.RemoveAll<ITransformationExecutor>();
            Services.AddTransient<ITransformationExecutor, T>();
            return this;
        }

        /// <summary>
        /// Allows configuring a custom <see cref="ITargetPageUriResolver"/> which resolves target page uri
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithTargetPageUriResolver<T>() where T : class, ITargetPageUriResolver
        {
            Services.RemoveAll<ITargetPageUriResolver>();
            Services.AddTransient<ITargetPageUriResolver, T>();
            return this;
        }

        /// <summary>
        /// Allows configuring the default transformation options for pages
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IPnPTransformationBuilder WithPageOptions(Action<PageTransformationOptions> options)
        {
            Services.Configure(options);
            return this;
        }

        /// <summary>
        /// Allows configuring a custom <see cref="IMappingProvider" /> to use for all transformations
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithMappingProvider<T>()
            where T : class, IMappingProvider
        {
            Services.RemoveAll<IMappingProvider>();
            Services.AddTransient<IMappingProvider, T>();

            return this;
        }

        /// <summary>
        /// Adds an object to intercept a page before the transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder AddPagePreTransformation<T>()
            where T : class, IPagePreTransformation
        {
            Services.AddTransient<IPagePreTransformation, T>();
            return this;
        }

        /// <summary>
        /// Adds an object to intercept a page after the transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder AddPagePostTransformation<T>()
            where T : class, IPagePostTransformation
        {
            Services.AddTransient<IPagePostTransformation, T>();
            return this;
        }

        /// <summary>
        /// Allows configuring a custom <see cref="ISourceProvider"/> to use in order to get the source items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithSourceProvider<T>()
            where T : class, ISourceProvider
        {
            Services.RemoveAll<ISourceProvider>();
            Services.AddTransient<ISourceProvider, T>();

            return this;
        }

        /// <summary>
        /// Allows configuring a custom <see cref="IAssetPersistenceProvider" /> that manages the persistence of assets onto a target persistence storage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithPersistenceProvider<T>()
            where T : class, IAssetPersistenceProvider
        {
            Services.RemoveAll<IAssetPersistenceProvider>();
            Services.AddTransient<IAssetPersistenceProvider, T>();

            return this;
        }
    }
}
