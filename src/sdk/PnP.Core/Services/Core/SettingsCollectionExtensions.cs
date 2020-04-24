using Microsoft.Extensions.DependencyInjection;
using System;

namespace PnP.Core.Services
{
    public static class SettingsCollectionExtensions
    {
        public static IServiceCollection AddSettings(this IServiceCollection collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            return collection.AddSingleton<ISettings, Settings>();
        }
    }
}
