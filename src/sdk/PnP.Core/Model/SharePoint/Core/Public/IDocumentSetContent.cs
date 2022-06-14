namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents the default content of document set in SharePoint.
    /// </summary>
    [ConcreteType(typeof(DocumentSetContent))]
    public interface IDocumentSetContent : IDataModel<IDocumentSetContent>
    {

        /// <summary>
        /// Content type information of the file.
        /// </summary>
        public IContentTypeInfo ContentType { get; set; }

        /// <summary>
        /// Name of the file in resource folder that should be added as a default content or a template in the document set.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Folder name in which the file will be placed when a new document set is created in the library.
        /// </summary>
        public string FolderName { get; set; }
    }
}
