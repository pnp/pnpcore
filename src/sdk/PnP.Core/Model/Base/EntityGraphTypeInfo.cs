using System;

namespace PnP.Core.Model
{
    internal class EntityGraphTypeInfo
    {
        /// <summary>
        /// The actual target type for which this attribute is valid
        /// </summary>
        internal Type Target { get; set; }

        /// <summary>
        /// Value of the id field used to load graph relationships (e.g. load lists from a given site)
        /// </summary>
        internal string Id { get; set; }

        /// <summary>
        /// Specifies if this class requires the Microsoft Graph beta endpoint
        /// </summary>
        internal bool Beta { get; set; }

        /// <summary>
        /// API call for a Graph get
        /// </summary>
        internal string Get { get; set; }

        /// <summary>
        /// API call for a Graph LINQ get
        /// </summary>
        internal string LinqGet { get; set; }

        /// <summary>
        /// Indicates the property used for the overflow field when a Graph query is used
        /// </summary>
        internal string OverflowProperty { get; set; }

        /// <summary>
        /// API call for a Graph update
        /// </summary>
        internal string Update { get; set; }

        /// <summary>
        /// API call for a Graph delete
        /// </summary>
        internal string Delete { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
