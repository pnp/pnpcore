using Microsoft.Extensions.DependencyInjection;
using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// Extension class for the IServiceCollection type to provide supporting methods for the AuthenticationProviderFactory service
    /// </summary>
    public static class AuthenticationProviderFactoryCollectionExtensions
    {
        /// <summary>
        /// Adds the <see cref="AuthenticationProviderFactory"/> service to the DI container
        /// </summary>
        /// <param name="collection">Collection of loaded services</param>
        /// <returns>Collection of loaded services</returns>
        public static IServiceCollection AddAuthenticationProviderFactory(this IServiceCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            // Add a SharePoint Online Context Factory service instance
            return collection
                .AddSettings()
                .AddAuthenticationServices();
        }

        /// <summary>
        /// Adds the <see cref="AuthenticationProviderFactory"/> service to the DI container
        /// </summary>
        /// <param name="collection">Collection of loaded services</param>
        /// <param name="options"><see cref="AuthenticationProviderFactory"/> options</param>
        /// <returns>Collection of loaded services</returns>
        public static IServiceCollection AddAuthenticationProviderFactory(this IServiceCollection collection, Action<AuthenticationProvidersOptions> options)
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
                .AddSettings()
                .AddAuthenticationServices();
        }

        private static IServiceCollection AddAuthenticationServices(this IServiceCollection collection)
        {
            return collection
                .AddOAuthAuthenticationProvider() // TODO: Remove this line and rely on PnP.Core.Auth external library
                .AddScoped<IAuthenticationProviderFactory, AuthenticationProviderFactory>();
        }
    }
}
