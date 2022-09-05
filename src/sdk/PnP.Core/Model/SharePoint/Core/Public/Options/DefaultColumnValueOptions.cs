namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Holds default column value properties
    /// </summary>
    public class DefaultColumnValueOptions
    {
        /// <summary>
        /// Folder relative path
        /// </summary>
        public string FolderRelativePath { get; set; }

        /// <summary>
        /// Field internal name
        /// </summary>
        public string FieldInternalName { get; set; }

        /// <summary>
        /// Default value for this field at the specified location
        /// </summary>
        public string DefaultValue { get; set; }
    }
}
