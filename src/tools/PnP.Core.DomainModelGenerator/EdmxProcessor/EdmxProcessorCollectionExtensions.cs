using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Services;
using System;

namespace PnP.M365.DomainModelGenerator
{
    public static class EdmxProcessorCollectionExtensions
    {
        public static IServiceCollection AddEdmxProcessor(this IServiceCollection collection, Action<EdmxProcessorOptions> options)
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

            // Add an Authentication Provider Factory service instance
            return collection
                .AddEdmxProcessorServices();
        }

        private static IServiceCollection AddEdmxProcessorServices(this IServiceCollection collection)
        {
            return collection
                .AddOAuthAuthenticationProvider()
                .AddScoped<IEdmxProcessor, EdmxProcessor>();
        }
    }
}
