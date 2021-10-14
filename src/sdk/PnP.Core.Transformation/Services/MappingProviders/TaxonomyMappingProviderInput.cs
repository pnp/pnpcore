using PnP.Core.Transformation.Services.Core;
using System;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a taxonomy mapping provider
    /// </summary>
    public class TaxonomyMappingProviderInput : BaseMappingProviderInput
    {
        /// <summary>
        /// Creates an instance for the context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="termId"></param>
        public TaxonomyMappingProviderInput(PageTransformationContext context, string termId) : base(context)
        {
            TermId = termId ?? throw new ArgumentNullException(nameof(termId));
        }

        /// <summary>
        /// Defines the source Term ID to map
        /// </summary>
        public string TermId { get; set; }
    }
}
