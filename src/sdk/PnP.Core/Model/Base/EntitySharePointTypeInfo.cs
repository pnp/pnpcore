using System;

namespace PnP.Core.Model
{
    internal sealed class EntitySharePointTypeInfo
    {
        /// <summary>
        /// Data store type when using REST
        /// </summary>
        internal string Type { get; set; }

        /// <summary>
        /// The actual target type for which this attribute is valid
        /// </summary>
        internal Type Target { get; set; }

        /// <summary>
        /// Uri that uniquely identifies this item via REST
        /// </summary>
        internal string Uri { get; set; }

        /// <summary>
        /// API call for a REST get
        /// </summary>
        internal string Get { get; set; }

        /// <summary>
        /// API call for a REST LINQ get
        /// </summary>
        internal string LinqGet { get; set; }

        /// <summary>
        /// Indicates the property used for the overflow field when a REST query is used
        /// </summary>
        internal string OverflowProperty { get; set; }

        /// <summary>
        /// API call for a REST update
        /// </summary>
        internal string Update { get; set; }

        /// <summary>
        /// API call for a REST delete
        /// </summary>
        internal string Delete { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
