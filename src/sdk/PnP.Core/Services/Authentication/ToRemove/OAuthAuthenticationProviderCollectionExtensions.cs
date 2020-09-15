using Microsoft.Extensions.DependencyInjection;
using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// Extension methods to assist with setting up the <see cref="IAuthenticationProvider"/>
    /// </summary>
    public static class OAuthAuthenticationProviderCollectionExtensions
    {
        /// <summary>
        /// Adds the <see cref="IAuthenticationProvider"/> to the collection of loaded services
        /// </summary>
        /// <param name="collection">Collection of loaded services</param>
        /// <returns>Collection of loaded services</returns>
        public static IServiceCollection AddOAuthAuthenticationProvider(this IServiceCollection collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            collection.AddScoped<IAuthenticationProvider, OAuthAuthenticationProvider>();
            collection.AddScoped<OAuthAuthenticationProvider, OAuthAuthenticationProvider>();

            return collection;
        }
    }
}
