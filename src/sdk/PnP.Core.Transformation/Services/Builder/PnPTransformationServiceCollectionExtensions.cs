using PnP.Core.Transformation.Services;
using PnP.Core.Transformation.Services.Builder;
using PnP.Core.Transformation.Services.Builder.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up PnP Transformation services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class PnPTransformationServiceCollectionExtensions
    {
        /// <summary>
        /// Configures PnP Transformation with default options
        /// </summary>
        /// <param name="services">The collection of services in an <see cref="IServiceCollection" /></param>
        /// <returns>A PnPTransformationBuilder instance</returns>
        public static IPnPTransformationBuilder AddPnPTransformation(this IServiceCollection services)
        {
            return AddPnPTransformation(services, null);
        }

        /// <summary>
        /// Configures PnP Transformation with custom options
        /// </summary>
        /// <param name="services">The collection of services in an <see cref="IServiceCollection" /></param>
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

            var builder = new PnPTransformationBuilder(services);
            // Set default implementations
            builder.AddDefaults();

            return builder;
        }

        /// <summary>
        /// Adds default implementations provided by the transformation framework
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IPnPTransformationBuilder AddDefaults(this IPnPTransformationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddTransient<IPageTransformator, DefaultPageTransformator>();
            builder.Services.TryAddTransient<IMappingProvider, DefaultMappingProvider>();
            builder.Services.TryAddTransient<ITransformationStateManager, InMemoryTransformationStateManager>();
            builder.Services.TryAddTransient<ITransformationExecutor, InProcessTransformationExecutor>();

            return builder;
        }
    }
}
