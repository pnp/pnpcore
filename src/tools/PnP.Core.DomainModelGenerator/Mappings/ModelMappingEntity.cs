using System.Collections.Generic;

namespace PnP.M365.DomainModelGenerator.Mappings
{
    /// <summary>
    /// Defines an entity mapping for converting EDMX metadata into the Domain Model Entity
    /// </summary>
    internal class ModelMappingEntity
    {
        /// <summary>
        /// The name of the Entity in the target Domain Model
        /// </summary>
        public string Name { get; set; }

        public List<ModelMappingTypeMapping> Mappings { get; set; }
    }
}
