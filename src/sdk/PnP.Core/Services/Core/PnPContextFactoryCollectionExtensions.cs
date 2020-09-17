using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// Extension class for the IServiceCollection type to provide supporting methods for the PnPContextFactory service
    /// </summary>
    public static class PnPContextFactoryCollectionExtensions
    {
        /// <summary>
        /// Adds the <see cref="PnPContextFactory"/> to the collection of services
        /// </summary>
        /// <param name="collection">Collection of loaded services</param>
        /// <returns>Collection of loaded services</returns>
        public static IServiceCollection AddPnPContextFactory(this IServiceCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            // Add a SharePoint Online Context Factory service instance
            return collection
#if !BLAZOR
                // TODO: Maybe we should review this part and move it into a method specific for Blazor, in the dedicated project/package
                .AddTelemetryServices()
#endif
                .AddHttpHandlers()
                .AddHttpClients()
                .AddPnPServices();
        }

        /// <summary>
        /// Adds the <see cref="PnPContextFactory"/> to the collection of services with options
        /// </summary>
        /// <param name="collection">Collection of loaded services</param>
        /// <param name="options"><see cref="PnPContextFactory"/> configuration options</param>
        /// <returns>Collection of loaded services</returns>
        public static IServiceCollection AddPnPContextFactory(this IServiceCollection collection, Action<PnPContextFactoryOptions> options)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            collection.Configure(options);

            // Add a PnP Context Factory service instance
            return collection
#if !BLAZOR
                .AddTelemetryServices()
#endif
                .AddHttpHandlers()
                .AddHttpClients()
                .AddPnPServices();
        }

        private static IServiceCollection AddHttpHandlers(this IServiceCollection collection)
        {
            collection.AddScoped<SharePointRestRetryHandler, SharePointRestRetryHandler>();
            collection.AddScoped<MicrosoftGraphRetryHandler, MicrosoftGraphRetryHandler>();

            return collection;
        }

        private static IServiceCollection AddHttpClients(this IServiceCollection collection)
        {
            collection.AddHttpClient<SharePointRestClient>()
                .AddHttpMessageHandler<SharePointRestRetryHandler>();
            collection.AddHttpClient<MicrosoftGraphClient>()
                .AddHttpMessageHandler<MicrosoftGraphRetryHandler>();

            return collection;
        }

        private static IServiceCollection AddPnPServices(this IServiceCollection collection)
        {
            return collection
                   .AddScoped<IPnPContextFactory, PnPContextFactory>();
        }

#if !BLAZOR
        private static IServiceCollection AddTelemetryServices(this IServiceCollection collection)
        {
            var globalOptionsService = collection.BuildServiceProvider().GetRequiredService<IOptions<PnPGlobalSettingsOptions>>();

            // Setup Azure App Insights
            // See https://github.com/microsoft/ApplicationInsights-Home/tree/master/Samples/WorkerServiceSDK/WorkerServiceSampleWithApplicationInsights as example
            return collection.AddApplicationInsightsTelemetryWorkerService(options =>
            {
                if (!globalOptionsService.Value.DisableTelemetry)
                {
                    // Production AppInsights
                    options.InstrumentationKey = "ffe6116a-bda0-4f0a-b0cf-d26f1b0d84eb";
                }
            });
        }
#endif
    }
}
