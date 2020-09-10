using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Builder
{
    /// <summary>
    /// Used to configure PnP Core SDK
    /// </summary>
    public class PnPCoreBuilder : IPnPCoreBuilder
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="services">The services being configured.</param>
        public PnPCoreBuilder(IServiceCollection services)
            => Services = services;

        /// <summary>
        /// The services being configured
        /// </summary>
        public virtual IServiceCollection Services { get; }
    }
}
