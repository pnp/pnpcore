using System;

namespace PnP.Core.Transformation.Model
{
    /// <summary>
    /// Defines a field to copy
    /// </summary>
    public class FieldData
    {
        /// <summary>
        /// Internal name of the field
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Id of the field
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Type of the field
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Value of the field instance
        /// </summary>
        public object Value { get; set; }
    }
}
