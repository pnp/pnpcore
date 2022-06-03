namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options for default document set content
    /// </summary>
    public class DocumentSetContentOptions
    {
        /// <summary>
        /// File to add as default content
        /// </summary>
        public IFile File { get; set; }

        /// <summary>
        /// Content type of the file to add as default content
        /// </summary>
        public string ContentTypeId { get; set; }

        /// <summary>
        /// File name of the file to be added as default content
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Folder name of the default content
        /// </summary>
        public string FolderName { get; set; }
    }
}
