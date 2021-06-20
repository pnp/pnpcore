using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a page layout mapping provider
    /// </summary>
    public class PageLayoutMappingProviderOutput: MappingProviderOutput
    {
        /// <summary>
        /// Provides the resulting Page Layout from the mapping
        /// </summary>
        public object PageLayout { get; set; } 
        
        // TODO: Define what to do with PageLayouts
        // Is it something related to SharePoint only? I think so ...
    }
}
