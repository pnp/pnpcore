using PnP.Core.Modernization.Services;
using PnP.Core.Modernization.Services.Builder;
using PnP.Core.Modernization.Services.Builder.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PnP.Core.Modernization.Services.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up PnP Modernization services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class PnPModernizationServiceCollectionExtensions
    {
        /// <summary>
        /// Configures PnP Modernization with default options
        /// </summary>
        /// <param name="services">The collection of services in an <see cref="IServiceCollection" /></param>
        /// <returns>A PnPModernizationBuilder instance</returns>
        public static IPnPModernizationBuilder AddPnPModernization(this IServiceCollection services)
        {
            return AddPnPModernization(services, null);
        }

        /// <summary>
        /// Configures PnP Modernization with custom options
        /// </summary>
        /// <param name="services">The collection of services in an <see cref="IServiceCollection" /></param>
        /// <param name="options">An Action to configure the PnP Modernization options</param>
        /// <returns>A PnPModernizationBuilder instance</returns>
        public static IPnPModernizationBuilder AddPnPModernization(this IServiceCollection services,
            Action<PnPModernizationOptions> options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options != null)
            {
                services.Configure(options);
            }

            var builder = new PnPModernizationBuilder(services);
            // Set default implementations
            builder.AddDefaults();

            return builder;
        }

        /// <summary>
        /// Adds default implementations provided by the modernization framework
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IPnPModernizationBuilder AddDefaults(this IPnPModernizationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddTransient<ITransformationStateManager, InMemoryTransformationStateManager>();
            builder.Services.TryAddTransient<ITransformationExecutor, SimpleTransformationExecutor>();

            return builder;
        }
    }
}
