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
        /// Sets a custom <see cref="ITransformationStateManager" /> to use to handle the state of a transformation process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithTransformationStateManager<T>()
            where T : class, ITransformationStateManager;

        /// <summary>
        /// Sets a custom <see cref="ITransformationDistiller" /> to defines a list of pages to transform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithTransformationDistiller<T>()
            where T : class, ITransformationDistiller;

        /// <summary>
        /// Sets a custom <see cref="IPageTransformator" /> to use to transform pages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithPageTransformator<T>()
            where T : class, IPageTransformator;

        /// <summary>
        /// Sets a custom <see cref="ITransformationExecutor" /> that manages the transformation of one or more pages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithTransformationExecutor<T>()
            where T : class, ITransformationExecutor;

        /// <summary>
        /// Customizes the default transformation options for the pages
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        IPnPTransformationBuilder WithPageOptions(Action<PageTransformationOptions> options);

        /// <summary>
        /// Sets a custom <see cref="IMappingProvider" /> to use for all transformations
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithMappingProvider<T>()
            where T : class, IMappingProvider;

        /// <summary>
        /// Adds a object to intercept a page before the transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder AddPagePreTransformation<T>()
            where T : class, IPagePreTransformation;

        /// <summary>
        /// Adds a object to intercept a page after the transformation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder AddPagePostTransformation<T>()
            where T : class, IPagePostTransformation;

        /// <summary>
        /// Sets the <see cref="ISourceProvider"/> to use in order to get the source items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IPnPTransformationBuilder WithSourceProvider<T>()
            where T : class, ISourceProvider;
    }
}
