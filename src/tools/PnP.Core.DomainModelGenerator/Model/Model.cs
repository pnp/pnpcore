using System.Collections.Generic;

namespace PnP.M365.DomainModelGenerator
{
    /// <summary>
    /// Defines the whole Model of entities
    /// </summary>
    internal class Model
    {
        /// <summary>
        /// Defines the collection of entities to generate
        /// </summary>
        //public List<ModelEntity> Entities { get; set; } = new List<ModelEntity>();
        public UnifiedModel SharePoint { get; set; }
        public UnifiedModel Graph { get; set; }
    }
}
