using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates a change to a <seealso cref="IListItem"/> object;
    /// </summary>
    /// <seealso cref="PnP.Core.Model.SharePoint.IChange" />
    [ConcreteType(typeof(ChangeItem))]
    public interface IChangeItem : IChange
    {
        public ChangeActivityType ActivityType { get; }

        /// <summary>
        /// Gets a value that identifies the content type ID of the the changed item.
        /// </summary>
        public IContentType ContentTypeId { get; }

        /// <summary>
        /// Gets a value that specifies the editor of the the changed item.
        /// </summary>
        public string Editor { get; }

        /// <summary>
        /// Gets a value that specifies the email of the editor of the the changed item.
        /// </summary>
        public string EditorEmailHint { get; }

        /// <summary>
        /// Gets a value that specifies the login name of the editor of the the changed item.
        /// </summary>
        public string EditorLoginName { get; }

        /// <summary>
        /// Gets a value that specifies the file system object type of the the changed item.
        /// </summary>
        public FileSystemObjectType FileSystemObjectType { get; }

        /// <summary>
        /// Gets a value that specifies the file type of the the changed item.
        /// </summary>
        public string FileType { get; }

        /// <summary>
        /// Gets a value that specifies the hash tag of the the changed item.
        /// </summary>
        public string Hashtag { get; }
        public bool Hidden { get; }

        /// <summary>
        /// Gets a value that identifies the changed item.
        /// </summary>
        public int ItemId { get; }

        /// <summary>
        /// Gets a value that identifies the list that contains the changed file.
        /// </summary>
        public Guid ListId { get; }

        /// <summary>
        /// Gets a value that specifies the title of the list that contains the changed item.
        /// </summary>
        public string ListTitle { get; }
        public bool MoveWasForRecycle { get; }
        public bool MoveWasForRestore { get; }

        /// <summary>
        /// Gets a value that specifies the server relative URL of the changed item.
        /// </summary>
        public string ServerRelativeUrl { get; }

        /// <summary>
        /// Gets a value that specifies the template type of the list that contains the changed item.
        /// </summary>
        public ListTemplateType TemplateType { get; }

        /// <summary>
        /// Gets a value that specifies the title of the changed item.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets a value that uniquely identifies the changed item.
        /// </summary>
        public Guid UniqueId { get; }

        /// <summary>
        /// Gets a value that identifies the web that contains the changed file.
        /// </summary>
        public Guid WebId { get; }

        #region TO IMPLEMENT
        //public ISharedWithUser SharedByUser { get; }
        //public IList<ISharedWithUser> SharedWithUsers { get; }
        #endregion
    }
}