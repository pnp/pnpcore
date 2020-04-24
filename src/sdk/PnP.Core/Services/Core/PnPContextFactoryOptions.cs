using System.Collections.Generic;

namespace PnP.Core.Services
{
    /// <summary>
    /// Defines the options to configure the PnPContextFactory object
    /// </summary>
    public class PnPContextFactoryOptions
    {
        /// <summary>
        /// Collection of configurations for PnPContextFactory
        /// </summary>
        public List<PnPContextFactoryOptionsConfiguration> Configurations { get; } = new List<PnPContextFactoryOptionsConfiguration>();
    }
}
