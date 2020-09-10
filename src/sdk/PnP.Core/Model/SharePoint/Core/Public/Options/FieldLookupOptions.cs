using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Available options for SharePoint lookup fields
    /// </summary>
    public class FieldLookupOptions : FieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies the internal field name of the field used as the lookup values.
        /// </summary>
        public string LookupFieldName { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the list identifier of the list that contains the field to use as the lookup values.
        /// </summary>
        public Guid LookupListId { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the GUID that identifies the site containing the list which contains the field used as the lookup values.
        /// </summary>
        public Guid LookupWebId { get; set; }
    }
}
