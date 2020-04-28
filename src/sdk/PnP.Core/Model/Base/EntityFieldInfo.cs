using System;
using System.Reflection;

namespace PnP.Core.Model
{
    /// <summary>
    /// Describes a field in our metadata model
    /// </summary>
    internal class EntityFieldInfo : ICloneable
    {
        /// <summary>
        /// Field name in the model
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// Datatype in the model
        /// </summary>
        internal Type DataType { get; set; }

        /// <summary>
        /// Property info for this field in a model ==> will be used by the json to model mapping
        /// </summary>
        internal PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// Is this a key field for SharePoint - used to prevent duplicate loading in collections
        /// </summary>
        internal bool IsSharePointKey { get; set; } = false;

        /// <summary>
        /// Is this a key field for SharePoint - used to prevent duplicate loading in collections
        /// </summary>
        internal bool IsGraphKey { get; set; } = false;

        /// <summary>
        /// Does this field need to be loaded
        /// </summary>
        internal bool Load { get; set; } = true;

        /// <summary>
        /// Is this an expandable field for REST
        /// </summary>
        internal bool SharePointExpandable { get; set; } = false;

        /// <summary>
        /// Is this an expandable field for Graph
        /// </summary>
        internal bool GraphExpandable { get; set; } = false;

        /// <summary>
        /// Expand this property when there's a default get (a get without specifying the fields to load)
        /// </summary>
        internal bool ExpandableByDefault { get; set; } = false;

        /// <summary>
        /// Name of this field when using SharePoint REST queries
        /// </summary>
        internal string SharePointName { get; set; }

        /// <summary>
        /// Name of this field when using Microsoft Graph queries
        /// </summary>
        internal string GraphName { get; set; }

        /// <summary>
        /// Json path that contains the value we need, relative to the object loaded via FieldName
        /// This works for simple JSON objects without arrays, delimiter will be a dot e.g. property.subproperty.subsub
        /// </summary>
        internal string SharePointJsonPath { get; set; }

        /// <summary>
        /// Json path that contains the value we need, relative to the object loaded via FieldName
        /// This works for simple JSON objects without arrays, delimiter will be a dot e.g. property.subproperty.subsub
        /// </summary>
        internal string GraphJsonPath { get; set; }

        /// <summary>
        /// Query to get a specific field, needed when a simple expand does not work
        /// </summary>
        internal string GraphGet { get; set; }

        /// <summary>
        /// Specifies if this property requires the Graph beta endpoint
        /// </summary>
        internal bool GraphBeta { get; set; }

        /// <summary>
        /// Force a call out to the custom mapping handler for this field / the default mapping logic will not be applied
        /// </summary>
        public bool SharePointUseCustomMapping { get; set; }

        /// <summary>
        /// Force a call out to the custom mapping handler for this field / the default mapping logic will not be applied
        /// </summary>
        public bool GraphUseCustomMapping { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }
}
