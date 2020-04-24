using System.Collections.Generic;

namespace PnP.M365.DomainModelGenerator
{
    /// <summary>
    /// Defines a property for a Domain Model Entity
    /// </summary>
    public class ModelProperty
    {
        /// <summary>
        /// The name of the property
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the property
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Defines whether the property can be expanded with $expand during the REST request
        /// </summary>
        public bool Expandable { get; set; }

        /// <summary>
        /// Defines the names of the properties in the target provider REST api
        /// </summary>
        public Dictionary<string, string> ProvidersProperties { get; } = new Dictionary<string, string>();
    }
}
