namespace PnP.Core.Model.SharePoint.Core.Public
{
    /// <summary>
    /// Available options for SharePoint URL fields
    /// </summary>
    public class FieldUrlOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies the display format for the value in the field.
        /// </summary>
        public UrlFieldFormatType DisplayFormat { get; set; }
    }
}
