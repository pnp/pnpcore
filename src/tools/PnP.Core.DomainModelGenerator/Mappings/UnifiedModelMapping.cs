using System.Collections.Generic;

namespace PnP.M365.DomainModelGenerator.Mappings
{
    internal class UnifiedModelMapping
    {
        /// <summary>
        /// Provider type
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

        /// <summary>
        /// Namespaces and folders of files to handle
        /// </summary>
        public List<UnifiedModelLocation> Locations { get; set; }

        /// <summary>
        /// Excluded types
        /// </summary>
        public List<UnifiedModelExclusion> ExcludedTypes { get; set; }
    }
}
