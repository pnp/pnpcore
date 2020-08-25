namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Available options for SharePoint Currency fields
    /// </summary>
    public class FieldCurrencyOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies the language code identifier (LCID) used to format the value of the field.
        /// </summary>
        public int? CurrencyLocaleId { get; set; }
    }
}
