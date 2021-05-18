using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Model.Classic;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a page layout mapping provider
    /// </summary>
    public class PageLayoutMappingProviderInput : MappingProviderInput
    {
        /// <summary>
        /// Defines the source Page Layout to map
        /// </summary>
        public PageLayout PageLayout { get; set; }
    }
}
