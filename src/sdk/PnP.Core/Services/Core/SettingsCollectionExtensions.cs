using Microsoft.Extensions.DependencyInjection;
using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// Add <see cref="ISettings"/> to the dependency injection container
    /// </summary>
    public static class SettingsCollectionExtensions
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="collection">Dependency injection container to add the <see cref="ISettings"/> to</param>
        /// <returns></returns>
        public static IServiceCollection AddSettings(this IServiceCollection collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return collection.AddSingleton<ISettings, Settings>();
        }
    }
}
