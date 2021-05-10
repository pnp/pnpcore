using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Modernization.Model.Classic;

namespace PnP.Core.Modernization.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a taxonomy mapping provider
    /// </summary>
    public class TaxonomyMappingProviderInput : MappingProviderInput
    {
        /// <summary>
        /// Defines the source Term ID to map
        /// </summary>
        public string TermId { get; set; }
    }
}
