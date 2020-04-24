using Microsoft.Extensions.DependencyInjection;
using System;

namespace PnP.Core.Services
{
    public static class OAuthAuthenticationProviderCollectionExtensions
    {
        public static IServiceCollection AddOAuthAuthenticationProvider(this IServiceCollection collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            collection.AddScoped<IAuthenticationProvider, OAuthAuthenticationProvider>();
            collection.AddScoped<OAuthAuthenticationProvider, OAuthAuthenticationProvider>();

            return collection;
        }
    }
}
