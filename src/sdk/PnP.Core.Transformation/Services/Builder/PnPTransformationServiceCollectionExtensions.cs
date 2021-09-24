using PnP.Core.Transformation.Services;
using PnP.Core.Transformation.Services.Builder;
using PnP.Core.Transformation.Services.Builder.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up PnP Transformation services in a <see cref="IServiceCollection" />.
    /// </summary>
    public static class PnPTransformationServiceCollectionExtensions
    {
        /// <summary>
        /// Configures PnP Transformation with default options
        /// </summary>
        /// <param name="services">The collection of services in a <see cref="IServiceCollection" /></param>
        /// <returns>A PnPTransformationBuilder instance</returns>
        public static IPnPTransformationBuilder AddPnPTransformation(this IServiceCollection services)
        {
            return AddPnPTransformation(services, null);
        }

        /// <summary>
        /// Configures PnP Transformation with custom options
        /// </summary>
        /// <param name="services">The collection of services in a <see cref="IServiceCollection" /></param>
        /// <param name="options">An Action to configure the PnP Transformation options</param>
        /// <returns>A PnPTransformationBuilder instance</returns>
        public static IPnPTransformationBuilder AddPnPTransformation(this IServiceCollection services,
            Action<PnPTransformationOptions> options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options != null)
            {
                services.Configure(options);
            }

            // TODO: Consider using the distributed one or providing an option to use the distributed one
            // Add the caching services
            services.AddMemoryCache(); 

            var builder = new PnPTransformationBuilder(services);

            // Set default implementations
            builder.AddDefaults();

            return builder;
        }

        /// <summary>
        /// Adds default implementations provided by the transformation framework
        /// </summary>
        /// <param name="builder">The services builder</param>
        /// <returns>A PnPTransformationBuilder instance</returns>
        public static IPnPTransformationBuilder AddDefaults(this IPnPTransformationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddTransient<IPageTransformator, DefaultPageTransformator>();
            builder.Services.TryAddTransient<IPageGenerator, DefaultPageGenerator>();
            builder.Services.TryAddTransient<ITransformationStateManager, InMemoryTransformationStateManager>();
            builder.Services.TryAddTransient<ITransformationExecutor, InProcessTransformationExecutor>();
            builder.Services.TryAddTransient<IAssetPersistenceProvider, FileSystemAssetPersistenceProvider>();
            builder.Services.TryAddTransient<TokenParser, TokenParser>();

            // Register all the token definitions as services
            var tokenDefinitionInterface = typeof(ITokenDefinition);
            var tokenDefinitions = typeof(PnPTransformationServiceCollectionExtensions).Assembly.GetTypes()
                .Where(t => tokenDefinitionInterface.IsAssignableFrom(t) && !t.IsInterface);

            foreach (var tokenDefinition in tokenDefinitions)
            {
                builder.Services.AddTransient(typeof(ITokenDefinition), tokenDefinition);
            }

            return builder;
        }
    }
}
