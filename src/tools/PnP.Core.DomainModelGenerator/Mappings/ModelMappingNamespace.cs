using System.Collections.Generic;

namespace PnP.M365.DomainModelGenerator.Mappings
{
    /// <summary>
    /// Defines a namespace mapping for converting EDMX metadata into the Domain Model Entity
    /// </summary>
    internal class ModelMappingNamespace
    {
        /// <summary>
        /// The name of the namespace to generate
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The collection of entity types to include in the namespace
        /// </summary>
        public List<ModelMappingEntity> Types { get; set; }

        /// <summary>
        /// A collection of namespaces
        /// </summary>
        public List<ModelMappingNamespace> Namespaces { get; set; }
    }
}
