namespace PnP.Core.Model.SharePoint.Core.Public
{
    /// <summary>
    /// Available options for SharePoint Text fields
    /// </summary>
    public class FieldTextOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets the maximum length of the text field.
        /// </summary>
        public int? MaxLength { get; set; }
    }
}
