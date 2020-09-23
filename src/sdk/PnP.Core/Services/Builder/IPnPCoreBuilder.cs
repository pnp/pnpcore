using Microsoft.Extensions.DependencyInjection;

namespace PnP.Core.Services.Builder
{
    /// <summary>
    /// Used to configure PnP Core SDK
    /// </summary>
    public interface IPnPCoreBuilder
    {
        /// <summary>
        /// Collection of services for Dependecy Injection
        /// </summary>
        IServiceCollection Services { get; }
    }
}
