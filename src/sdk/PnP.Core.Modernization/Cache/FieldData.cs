using System;

namespace PnP.Core.Modernization.Cache
{
    /// <summary>
    /// Field data used to transfer information about a field
    /// </summary>
    [Serializable]
    public class FieldData
    {
        /// <summary>
        /// Internal name of the field
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Id of the field
        /// </summary>
        public Guid FieldId { get; set; }

        /// <summary>
        /// Type of the field
        /// </summary>
        public string FieldType { get; set; }
    }
}
