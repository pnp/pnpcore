using PnP.Core.Services;
using PnP.Core.Services.Builder;
using PnP.Core.Services.Builder.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up PnP Core SDK services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class PnPCoreServiceCollectionExtensions
    {
        /// <summary>
        /// Configures PnP Core SDK with default options
        /// </summary>
        /// <param name="services">The collection of services in an <see cref="IServiceCollection" /></param>
        /// <returns>A PnPCoreBuilder instance</returns>
        public static IPnPCoreBuilder AddPnPCore(this IServiceCollection services)
        {
            return AddPnPCore(services, null);
        }

        /// <summary>
        /// Configures PnP Core SDK with default options
        /// </summary>
        /// <param name="services">The collection of services in an <see cref="IServiceCollection" /></param>
        /// <param name="options">An Action to configure the Authentication options</param>
        /// <returns>A PnPCoreBuilder instance</returns>
        public static IPnPCoreBuilder AddPnPCore(this IServiceCollection services,
            Action<PnPCoreOptions> options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options != null)
            {
                services.Configure(options);
            }

            services.ConfigureOptions<PnPCoreOptionsConfigurator>();
            services.AddPnPContextFactory();

            return new PnPCoreBuilder(services);
        }
    }
}
