using System.Collections.Generic;

namespace PnP.M365.DomainModelGenerator
{
    public class ModelEntity
    {
        /// <summary>
        /// The name of the type that will be generated
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// The namespace of the type that will be generated
        /// </summary>
        public string Namespace { get; set; }

        public List<ModelProperty> Properties { get; } = new List<ModelProperty>();

        /// <summary>
        /// Dictionary of URIs to consume the various target providers for the current entity
        /// </summary>
        public Dictionary<string, string> ProvidersUris { get; } = new Dictionary<string, string>();
    }
}
