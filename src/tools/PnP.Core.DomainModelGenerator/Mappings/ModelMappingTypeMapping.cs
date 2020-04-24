using System.Collections.Generic;

namespace PnP.M365.DomainModelGenerator.Mappings
{
    /// <summary>
    /// Defines the mapping information between a Domain Model Entity and the metadata source model
    /// </summary>
    internal class ModelMappingTypeMapping
    {
        /// <summary>
        /// The name of the metadata provider
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// The name of the entity in the provider's metadata
        /// </summary>
        public ModelMappingProviderEntity ProviderEntity { get; set; }

        /// <summary>
        /// The list of properties defined for mapping the Domain Model Entity with the provider's metadata entity
        /// </summary>
        public List<ModelMappingTypeMappingProperties> Properties { get; set; }

        /// <summary>
        /// The list of properties that will not be imported from the provider's metadata
        /// </summary>
        public List<string> ExcludedProperties { get; set; }

        /// <summary>
        /// The URL of the REST API to consume the entity with the target provider
        /// </summary>
        public string RestUrl { get; set; }
    }
}
