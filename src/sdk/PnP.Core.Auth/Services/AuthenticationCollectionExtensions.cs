using PnP.Core.Services;
using System;
using PnP.Core.Auth;
using PnP.Core.Auth.Services;
using PnP.Core.Auth.Services.Builder.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to assist with setting up the <see cref="IAuthenticationProvider"/> infrastructure
    /// </summary>
    public static class AuthenticationCollectionExtensions
    {
        /// <summary>
        /// Adds all the <see cref="IAuthenticationProvider"/> flavors to the collection of loaded services
        /// </summary>
        /// <param name="collection">Collection of loaded services</param>
        /// <returns>Collection of loaded services</returns>
        public static IServiceCollection AddPnPCoreAuthentication(this IServiceCollection collection)
        {
            return AddPnPCoreAuthentication(collection, null);
        }

        /// <summary>
        /// Adds all the <see cref="IAuthenticationProvider"/> flavors to the collection of loaded services
        /// </summary>
        /// <param name="collection">Collection of loaded services</param>
        /// <param name="options"><see cref="PnPCoreAuthenticationOptions"/> options to use for configuration</param>
        /// <returns>Collection of loaded services</returns>
        public static IServiceCollection AddPnPCoreAuthentication(this IServiceCollection collection, Action<PnPCoreAuthenticationOptions> options)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (options != null)
            {
                collection.Configure(options);
            }

            AddAuthenticationProviders(collection);
            collection.ConfigureOptions<AuthenticationProvidersOptionsConfigurator>();

            return collection;
        }

        private static void AddAuthenticationProviders(IServiceCollection collection)
        {
            // Add the service farctory for the authentication providers
            collection.AddTransient<IAuthenticationProviderFactory, AuthenticationProviderFactory>();
            collection.AddTransient<AuthenticationProviderFactory, AuthenticationProviderFactory>();

            // Add the service for AspNetCoreAuthenticationProvider
            collection.AddTransient<IAuthenticationProvider, AspNetCoreAuthenticationProvider>();
            collection.AddTransient<AspNetCoreAuthenticationProvider, AspNetCoreAuthenticationProvider>();

            // Add the service for CredentialManagerAuthenticationProvider
            collection.AddTransient<IAuthenticationProvider, CredentialManagerAuthenticationProvider>();
            collection.AddTransient<CredentialManagerAuthenticationProvider, CredentialManagerAuthenticationProvider>();

            // Add the service for OnBehalfOfAuthenticationProvider
            collection.AddTransient<IAuthenticationProvider, OnBehalfOfAuthenticationProvider>();
            collection.AddTransient<OnBehalfOfAuthenticationProvider, OnBehalfOfAuthenticationProvider>();

            // Add the service for UsernamePasswordAuthenticationProvider
            collection.AddTransient<IAuthenticationProvider, UsernamePasswordAuthenticationProvider>();
            collection.AddTransient<UsernamePasswordAuthenticationProvider, UsernamePasswordAuthenticationProvider>();

            // Add the service for InteractiveAuthenticationProvider
            collection.AddTransient<IAuthenticationProvider, InteractiveAuthenticationProvider>();
            collection.AddTransient<InteractiveAuthenticationProvider, InteractiveAuthenticationProvider>();

            // Add the service for X509CertificateAuthenticationProvider
            collection.AddTransient<IAuthenticationProvider, X509CertificateAuthenticationProvider>();
            collection.AddTransient<X509CertificateAuthenticationProvider, X509CertificateAuthenticationProvider>();
        }
    }

}
