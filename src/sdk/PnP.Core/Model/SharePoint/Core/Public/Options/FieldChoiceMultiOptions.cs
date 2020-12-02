namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Available options for SharePoint Multi Choice fields
    /// </summary>
    public class FieldChoiceMultiOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies whether the field can accept values other than those specified in Choices.
        /// </summary>
        public bool? FillInChoice { get; set; }

        /// <summary>
        /// Gets or sets the default choice value 
        /// </summary>
        public string DefaultChoice { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies values that are available for selection in the field.
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public string[] Choices { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
    }
}
