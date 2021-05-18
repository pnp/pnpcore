using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Model.Modern;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a taxonomy mapping provider
    /// </summary>
    public class TaxonomyMappingProviderOutput : MappingProviderOutput
    {
        /// <summary>
        /// Defines the target Term ID from the mapping
        /// </summary>
        public string TermId { get; set; }
    }
}
