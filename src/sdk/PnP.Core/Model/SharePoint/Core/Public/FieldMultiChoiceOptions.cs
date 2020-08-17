namespace PnP.Core.Model.SharePoint.Core.Public
{
    /// <summary>
    /// Available options for SharePoint Multi Choice fields
    /// </summary>
    public class FieldMultiChoiceOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies whether the field can accept values other than those specified in Choices.
        /// </summary>
        public bool? FillInChoice { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies values that are available for selection in the field.
        /// </summary>
        public string[] Choices { get; set; }
    }
}
