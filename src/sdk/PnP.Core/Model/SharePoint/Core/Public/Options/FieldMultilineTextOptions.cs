namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Available options for SharePoint Multiline Text fields
    /// </summary>
    public class FieldMultilineTextOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies whether a hyperlink is allowed as a value of the field.
        /// </summary>
        public bool? AllowHyperlink { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether all changes to the value of the field are displayed in list forms.
        /// </summary>
        public bool? AppendOnly { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the number of lines of text to display for the field.
        /// </summary>
        public int? NumberOfLines { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the field supports a subset of rich formatting.
        /// </summary>
        public bool? RestrictedMode { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the field supports rich formatting.
        /// </summary>
        public bool? RichText { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the field supports unlimited length in document libraries.
        /// </summary>
        public bool? UnlimitedLengthInDocumentLibrary { get; set; }

    }
}
