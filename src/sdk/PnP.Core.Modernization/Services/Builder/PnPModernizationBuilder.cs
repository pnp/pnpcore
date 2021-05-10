using Microsoft.Extensions.DependencyInjection;

namespace PnP.Core.Modernization.Services.Builder
{
    /// <summary>
    /// Used to configure PnP Core Modernization
    /// </summary>
    public class PnPModernizationBuilder : IPnPModernizationBuilder
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="services">The services being configured.</param>
        public PnPModernizationBuilder(IServiceCollection services)
            => Services = services;

        /// <summary>
        /// The services being configured
        /// </summary>
        public virtual IServiceCollection Services { get; }
    }
}
