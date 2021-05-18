using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;

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
        /// Sets a custom <see cref="ITransformationStateManager" /> to use to handle the state of a transformation process
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
        /// Sets a custom <see cref="ITransformationDistiller" /> to defines a list of pages to transform
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
        /// Sets a custom <see cref="IPageTransformator" /> to use to transform pages
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
        /// Sets a custom <see cref="ITransformationExecutor" /> that manages the transformation of one or more pages
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
        /// Customizes the default transformation options for the pages
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IPnPTransformationBuilder WithPageOptions(Action<PageTransformationOptions> options)
        {
            Services.Configure(options);
            return this;
        }

        /// <summary>
        /// Sets a custom <see cref="IWebPartMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithWebPartMappingProvider<T>()
            where T : class, IWebPartMappingProvider
        {
            return WithProvider<T>((o, t) => o.WebPartMappingProvider = t);
        }

        /// <summary>
        /// Sets a custom <see cref="IPageLayoutMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithPageLayoutMappingProvider<T>() where T : class, IPageLayoutMappingProvider
        {
            return WithProvider<T>((o, t) => o.PageLayoutMappingProvider = t);
        }

        /// <summary>
        /// Sets a custom <see cref="ITaxonomyMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithTaxonomyMappingProvider<T>()
            where T : class, ITaxonomyMappingProvider
        {
            return WithProvider<T>((o, t) => o.TaxonomyMappingProvider = t);
        }

        /// <summary>
        /// Sets a custom <see cref="IMetadataMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithMetadataMappingProvider<T>()
            where T : class, IMetadataMappingProvider
        {
            return WithProvider<T>((o, t) => o.MetadataMappingProvider = t);
        }

        /// <summary>
        /// Sets a custom <see cref="IUrlMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithUrlMappingProvider<T>()
            where T : class, IUrlMappingProvider
        {
            return WithProvider<T>((o, t) => o.UrlMappingProvider = t);
        }

        /// <summary>
        /// Sets a custom <see cref="IHtmlMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithHtmlMappingProvider<T>()
            where T : class, IHtmlMappingProvider
        {
            return WithProvider<T>((o, t) => o.HtmlMappingProvider = t);
        }

        /// <summary>
        /// Sets a custom <see cref="IUserMappingProvider" /> to use for the default transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder WithUserMappingProvider<T>()
            where T : class, IUserMappingProvider
        {
            return WithProvider<T>((o, t) => o.UserMappingProvider = t);
        }

        /// <summary>
        /// Sets a custom <see cref="IMappingProvider" /> to use for all transformations
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
        /// Adds a object to intercept a page before the transformation
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
        /// Adds a object to intercept a page after the transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IPnPTransformationBuilder AddPagePostTransformation<T>()
            where T : class, IPagePostTransformation
        {
            Services.AddTransient<IPagePostTransformation, T>();
            return this;
        }

        private IPnPTransformationBuilder WithProvider<T>(Action<PageTransformationOptions, T> setAction)
            where T : class
        {
            Services.RemoveAll<T>();
            Services.AddTransient<T>();
            Services.AddOptions<PageTransformationOptions>().Configure(setAction);

            return this;
        }

    }
}
