using System.Collections.Generic;

namespace PnP.M365.DomainModelGenerator.Mappings
{
    /// <summary>
    /// Defines the mapping rules for Entities defined in the Model
    /// </summary>
    internal class ModelMappings
    {
        /// <summary>
        /// The list of providers configured in the mappings
        /// </summary>
        public List<ModelMappingProvider> Providers { get; set; }

        /// <summary>
        /// The namespaces to generate in the Domain Model
        /// </summary>
        public List<ModelMappingNamespace> Namespaces { get; set; }
    }
}
