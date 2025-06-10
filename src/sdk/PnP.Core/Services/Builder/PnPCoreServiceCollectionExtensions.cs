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
        /// <remarks>
        /// <para>
        /// Optional additional configuration can be provided for <see cref="SharePointRestClient"/> and/or <see cref="MicrosoftGraphClient"/>
        /// </para>
        /// </remarks>
        /// <param name="services">The collection of services in an <see cref="IServiceCollection" /></param>
        /// <param name="configureSharePointRest">additional configuration to <see cref="SharePointRestClient"/></param>
        /// <param name="configureMicrosoftGraph">additional configuration to <see cref="MicrosoftGraphClient"/></param>
        /// <returns>A PnPCoreBuilder instance</returns>
        public static IPnPCoreBuilder AddPnPCore(this IServiceCollection services,
            Action<IHttpClientBuilder> configureSharePointRest = null,
            Action<IHttpClientBuilder> configureMicrosoftGraph = null)
        {
            return AddPnPCore(services, null, configureSharePointRest, configureMicrosoftGraph);
        }

        /// <summary>
        /// Configures PnP Core SDK with custom options
        /// </summary>
        /// <remarks>
        /// <para>
        /// Optional additional configuration can be provided for <see cref="SharePointRestClient"/> and/or <see cref="MicrosoftGraphClient"/>
        /// </para>
        /// </remarks>
        /// <param name="services">The collection of services in an <see cref="IServiceCollection" /></param>
        /// <param name="options">An Action to configure the PnP Core options</param>
        /// <param name="configureSharePointRest">additional configuration to <see cref="SharePointRestClient"/></param>
        /// <param name="configureMicrosoftGraph">additional configuration to <see cref="MicrosoftGraphClient"/></param>
        /// <returns>A PnPCoreBuilder instance</returns>
        public static IPnPCoreBuilder AddPnPCore(this IServiceCollection services,
            Action<PnPCoreOptions> options,
            Action<IHttpClientBuilder> configureSharePointRest = null,
            Action<IHttpClientBuilder> configureMicrosoftGraph = null)
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
            services.AddPnPContextFactory(configureSharePointRest, configureMicrosoftGraph);

            return new PnPCoreBuilder(services);
        }
    }
}
