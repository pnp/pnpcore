namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Available options for SharePoint user fields
    /// </summary>
    public class FieldUserOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies whether to allow multiple values.
        /// </summary>
        public bool? AllowMultipleValues { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether to display the name of the user in a survey list.
        /// </summary>
        public bool? AllowDisplay { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether presence is enabled on the field.
        /// </summary>
        public bool? Presence { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the identifier of the SharePoint group whose members can be selected as values of the field.
        /// </summary>
        public int? SelectionGroup { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether users and groups or only users can be selected.
        /// </summary>
        public FieldUserSelectionMode SelectionMode { get; set; }
    }
}
