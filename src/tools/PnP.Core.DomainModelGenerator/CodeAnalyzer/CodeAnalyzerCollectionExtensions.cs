using Microsoft.Extensions.DependencyInjection;
using System;

namespace PnP.M365.DomainModelGenerator.CodeAnalyzer
{
    public static class CodeAnalyzerCollectionExtensions
    {
        public static IServiceCollection AddCodeAnalyzer(this IServiceCollection collection, Action<CodeAnalyzerOptions> options)
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
                .AddCodeAnalyzerServices();
        }

        private static IServiceCollection AddCodeAnalyzerServices(this IServiceCollection collection)
        {
            return collection
                .AddScoped<ICodeAnalyzer, CodeAnalyzer>();
        }
    }
}
