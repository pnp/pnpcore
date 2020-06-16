using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;

namespace PnP.Core.Test.Services
{
    /// <summary>
    /// Extension class for the IServiceCollection type to provide supporting methods for the TestPnPContextFactory service
    /// </summary>
    public static class TestPnPContextFactoryCollectionExtensions
    {
        public static IServiceCollection AddTestPnPContextFactory(this IServiceCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            // Add a SharePoint Online Context Factory service instance
            return collection
                .AddSettings()
                .AddTelemetryServices()
                .AddHttpClients()
                .AddPnPServices();
        }

        public static IServiceCollection AddTestPnPContextFactory(this IServiceCollection collection, Action<PnPContextFactoryOptions> options)
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
                .AddSettings()
                .AddTelemetryServices()
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
                   .AddScoped<IPnPContextFactory, TestPnPContextFactory>();
        }

        private static IServiceCollection AddTelemetryServices(this IServiceCollection collection)
        {
            var settingsService = collection.BuildServiceProvider().GetRequiredService<ISettings>();

            // Setup Azure App Insights
            // See https://github.com/microsoft/ApplicationInsights-Home/tree/master/Samples/WorkerServiceSDK/WorkerServiceSampleWithApplicationInsights as example
            return collection.AddApplicationInsightsTelemetryWorkerService(options =>
            {
                if (!settingsService.DisableTelemetry && !TestCommon.RunningInGitHubWorkflow())
                {
                        // Test AppInsights
                        options.InstrumentationKey = "6073339d-9e70-4004-9ff7-1345316ade97";
                }
            });

        }
    }
}
