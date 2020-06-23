using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.M365.DomainModelGenerator
{
    internal class UnifiedModelProperty
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
        /// Is this property a navigation property
        /// </summary>
        public bool NavigationProperty { get; set; }

        /// <summary>
        /// Is the navigation property a collection
        /// </summary>
        public bool NavigationPropertyIsCollection { get; set; }

        /// <summary>
        /// Are we skipping this property during code generation time?
        /// </summary>
        public bool Skip { get; set; }
    }
}
