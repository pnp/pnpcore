namespace PnP.M365.DomainModelGenerator.Mappings
{
    /// <summary>
    /// Defines a Provider for the mapping
    /// </summary>
    internal class ModelMappingProvider
    {
        /// <summary>
        /// The name of the metadata provider
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// The XML Namespace of the Edmx element in the metadata file
        /// </summary>
        public string EdmxNamespace { get; set; }

        /// <summary>
        /// The XML Namespace of the schema element in the metadata file
        /// </summary>
        public string SchemaNamespace { get; set; }
    }
}
