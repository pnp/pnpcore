using System;

namespace PnP.Core.Model
{
    /// <summary>
    /// Attribute used to decorate model classes that can be loaded via Microsoft Graph
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class GraphTypeAttribute: Attribute
    {
        /// <summary>
        /// Defines the field that serves as the id field in Graph, typically this is the "id" field
        /// </summary>
        public string GraphId { get; set; }

        /// <summary>
        /// URI that identifies an object in Microsoft Graph, used for get, update and delete requests
        /// </summary>
        public string GraphUri { get; set; }

        /// <summary>
        /// Get API URI, overrides the global GraphURI when specified
        /// </summary>
        public string GraphGet { get; set; }

        /// <summary>
        /// Update API URI, overrides the global GraphURI when specified
        /// </summary>
        public string GraphUpdate { get; set; }

        /// <summary>
        /// Delete API URI, overrides the global GraphURI when specified
        /// </summary>
        public string GraphDelete { get; set; }

        /// <summary>
        /// Model property name that will hold dynamic data (e.g. List Item property/value pairs)
        /// </summary>
        public string GraphOverflowProperty { get; set; }
    }
}
