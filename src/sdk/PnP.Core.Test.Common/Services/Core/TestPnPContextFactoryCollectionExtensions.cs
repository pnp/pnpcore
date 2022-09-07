using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Services;
using PnP.Core.Services.Builder.Configuration;
using PnP.Core.Services.Core;
using System;
using System.Net;
using System.Net.Http;

namespace PnP.Core.Test.Common.Services
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
            return AddTestPnPContextFactory(collection, null);
        }

        public static IServiceCollection AddTestPnPContextFactory(this IServiceCollection collection, Action<PnPContextFactoryOptions> options)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (options != null)
            {
                collection.Configure(options);
            }

            collection.ConfigureOptions<PnPCoreOptionsConfigurator>();

            // Add a PnP Context Factory service instance
            return collection
                .AddHttpHandlers()
                .AddHttpClients()
                .AddPnPServices();
        }

        private static IServiceCollection AddHttpHandlers(this IServiceCollection collection)
        {
            collection.AddTransient<SharePointRestRetryHandler, SharePointRestRetryHandler>();
            collection.AddTransient<MicrosoftGraphRetryHandler, MicrosoftGraphRetryHandler>();

            return collection;
        }

        private static IServiceCollection AddHttpClients(this IServiceCollection collection)
        {
            collection.AddHttpClient<SharePointRestClient>()
                .AddHttpMessageHandler<SharePointRestRetryHandler>()
                    // We use cookies by adding them to the header which works great when used from Core framework,
                    // however when running the .NET Standard 2.0 version from .NET Framework we explicetely have to
                    // tell the http client to not use the default (empty) cookie container
                    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                    {
                        UseCookies = false,
                        AutomaticDecompression = DecompressionMethods.All,
                        
                    });
            collection.AddHttpClient<MicrosoftGraphClient>()
                .AddHttpMessageHandler<MicrosoftGraphRetryHandler>()
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.All,
                });

            return collection;
        }

        private static IServiceCollection AddPnPServices(this IServiceCollection collection)
        {
            return collection
                   .AddTransient<IPnPTestContextFactory, TestPnPContextFactory>()
                   .AddSingleton<EventHub>()
                   .AddSingleton<RateLimiter>();
        }
    }
}
