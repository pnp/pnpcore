namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Available options for SharePoint Calculated fields
    /// </summary>
    public class FieldCalculatedOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies the language code identifier (LCID) used to format the value of the field.
        /// </summary>
        public int? CurrencyLocaleId { get; set; }

        /// <summary>
        /// Gets or sets the type of date and time format that is used in the field.
        /// </summary>
        public DateTimeFieldFormatType? DateFormat { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the formula for the field.
        /// </summary>
        public string Formula { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the output format for the field.
        /// </summary>
        public FieldType OutputType { get; set; }

        /// <summary>
        /// Gets or sets whether the field must be shown as percentage.
        /// </summary>
        public bool? ShowAsPercentage { get; set; }

        ///// <summary>
        ///// Gets or sets the display format of the field.
        ///// Although in the docs as usable parameters, DisplayFormat is always -1 in response for number fields
        ///// </summary>
        //public int? DisplayFormat { get; set; }
    }
}
