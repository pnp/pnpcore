using System;

namespace PnP.Core.Model
{
    /// <summary>
    /// Attribute used to decorate model classes that can be loaded via SharePoint REST
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class SharePointTypeAttribute: Attribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="sharePointType">SharePoint REST type</param>
        internal SharePointTypeAttribute(string sharePointType)
        {
            Type = sharePointType;
        }

        /// <summary>
        /// Defines the OData name of this SharePoint REST type (e.g. SP.Web)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The actual target type for which this attribute is valid
        /// </summary>
        public Type Target { get; set; }

        /// <summary>
        /// URI that identifies an object in SharePoint REST, used for get, update and delete requests
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Get LINQ query API URI, overrides the global SharePointURI when specified
        /// </summary>
        public string LinqGet { get; set; }

        /// <summary>
        /// Get API URI, overrides the global SharePointURI when specified
        /// </summary>
        public string Get { get; set; }

        /// <summary>
        /// Update API URI, overrides the global SharePointURI when specified
        /// </summary>
        public string Update { get; set; }

        /// <summary>
        /// Delete API URI, overrides the global SharePointURI when specified
        /// </summary>
        public string Delete { get; set; }

        /// <summary>
        /// Model property name that will hold dynamic data (e.g. List Item property/value pairs)
        /// </summary>
        public string OverflowProperty { get; set; }
    }
}
