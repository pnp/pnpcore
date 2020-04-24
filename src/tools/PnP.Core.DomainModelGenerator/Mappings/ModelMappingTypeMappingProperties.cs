namespace PnP.M365.DomainModelGenerator.Mappings
{
    /// <summary>
    /// Defines the mapping between a property in the Domain Model and a property in the provider's metadata
    /// </summary>
    internal class ModelMappingTypeMappingProperties
    {
        /// <summary>
        /// The name of the property in the Domain Model
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of the property in the provider's metadata definition
        /// </summary>
        public string ProviderName { get; set; }
    }
}
