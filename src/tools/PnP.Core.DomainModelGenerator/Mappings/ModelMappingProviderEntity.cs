namespace PnP.M365.DomainModelGenerator
{
    /// <summary>
    /// Defines the base coordinates of an entity defined in a metatada provider
    /// </summary>
    internal class ModelMappingProviderEntity
    {
        /// <summary>
        /// The name of the Namespace of the entity in the provider's metadata
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// The Name of the entity in the provider's metadata
        /// </summary>
        public string Name { get; set; }
    }
}
