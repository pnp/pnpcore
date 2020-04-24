using Microsoft.Extensions.DependencyInjection;
using System;

namespace PnP.M365.DomainModelGenerator
{
    public static class CodeGeneratorCollectionExtensions
    {
        public static IServiceCollection AddCodeGenerator(this IServiceCollection collection, Action<CodeGeneratorOptions> options)
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
                .AddCodeGeneratorServices();
        }

        private static IServiceCollection AddCodeGeneratorServices(this IServiceCollection collection)
        {
            return collection
                .AddScoped<ICodeGenerator, CodeGenerator>();
        }
    }
}
