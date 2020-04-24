using Microsoft.Extensions.DependencyInjection;
using System;

namespace PnP.Core.Services
{
    public static class FakeAuthenticationProviderCollectionExtensions
    {
        public static IServiceCollection AddFakeAuthenticationProvider(this IServiceCollection collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            collection.AddScoped<IAuthenticationProvider, FakeAuthenticationProvider>();
            collection.AddScoped<FakeAuthenticationProvider, FakeAuthenticationProvider>();

            return collection;
        }
    }
}
