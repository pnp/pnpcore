using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ListItem object
    /// </summary>
    [ConcreteType(typeof(ListItem))]
    public interface IListItem : IDataModel<IListItem>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool CommentsDisabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Id { get; }

        #endregion

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int CommentsDisabledScope { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int FileSystemObjectType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string IconOverlay { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServerRedirectedEmbedUri { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServerRedirectedEmbedUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Client_Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IAttachmentCollection AttachmentFiles { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IContentType ContentType { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IDlpPolicyTip GetDlpPolicyTip { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFieldStringValues FieldValuesAsHtml { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFieldStringValues FieldValuesAsText { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFieldStringValues FieldValuesForEdit { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFile File { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFolder Folder { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IlikedByInformation LikedByInformation { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IList ParentList { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IPropertyValues Properties { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IListItemVersionCollection Versions { get; }

        #endregion

    }
}
