using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Modernization.Model.Modern;

namespace PnP.Core.Modernization.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a page layout mapping provider
    /// </summary>
    public class PageLayoutMappingProviderOutput: MappingProviderOutput
    {
        /// <summary>
        /// Provides the resulting Page Layout from the mapping
        /// </summary>
        public PageLayout PageLayout { get; set; }
    }
}
