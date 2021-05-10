using Microsoft.Extensions.DependencyInjection;

namespace PnP.Core.Modernization.Services.Builder
{
    /// <summary>
    /// Used to configure PnP Core Modernization
    /// </summary>
    public interface IPnPModernizationBuilder
    {
        /// <summary>
        /// Collection of services for Dependecy Injection
        /// </summary>
        IServiceCollection Services { get; }
    }
}
