using System;

namespace PnP.Core.Model
{
    /// <summary>
    /// Attribute used to decorate model classes that can be loaded via SharePoint REST
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class SharePointTypeAttribute: Attribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="sharePointType">SharePoint REST type</param>
        internal SharePointTypeAttribute(string sharePointType)
        {
            SharePointType = sharePointType;
        }

        /// <summary>
        /// Defines the OData name of this SharePoint REST type (e.g. SP.Web)
        /// </summary>
        public string SharePointType { get; set; }

        /// <summary>
        /// URI that identifies an object in SharePoint REST, used for get, update and delete requests
        /// </summary>
        public string SharePointUri { get; set; }

        /// <summary>
        /// Get API URI, overrides the global SharePointURI when specified
        /// </summary>
        public string SharePointGet { get; set; }

        /// <summary>
        /// Update API URI, overrides the global SharePointURI when specified
        /// </summary>
        public string SharePointUpdate { get; set; }

        /// <summary>
        /// Delete API URI, overrides the global SharePointURI when specified
        /// </summary>
        public string SharePointDelete { get; set; }

        /// <summary>
        /// Model property name that will hold dynamic data (e.g. List Item property/value pairs)
        /// </summary>
        public string SharePointOverflowProperty { get; set; }
    }
}
