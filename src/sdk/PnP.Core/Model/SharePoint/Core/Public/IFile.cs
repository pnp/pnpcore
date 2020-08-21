using PnP.Core.Services;
using System;
using System.Security.Policy;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a File object
    /// </summary>
    [ConcreteType(typeof(File))]
    public interface IFile : IDataModel<IFile>, IDataModelUpdate, IDataModelDelete
    {
        /// <summary>
        /// Gets a value that returns the comment used when a document is checked into a document library.
        /// </summary>
        public string CheckInComment { get; }

        /// <summary>
        /// Gets a value that specifies the type of check out associated with the file.
        /// </summary>
        public int CheckOutType { get; }

        /// <summary>
        /// Returns internal version of content, used to validate document equality for read purposes.
        /// </summary>
        public string ContentTag { get; }

        /// <summary>
        /// Gets a value that specifies the customization status of the file.
        /// </summary>
        public CustomizedPageStatus CustomizedPageStatus { get; }

        /// <summary>
        /// Gets the id of the list containing the file.
        /// </summary>
        public Guid ListId { get; }

        /// <summary>
        /// Gets a value that specifies the ETag value.
        /// </summary>
        public string ETag { get; }

        /// <summary>
        /// Gets a value that specifies whether the file exists.
        /// </summary>
        public bool Exists { get; }

        /// <summary>
        /// Gets or sets whether Irm is enabled on the file.
        /// </summary>
        public bool IrmEnabled { get; set; }

        /// <summary>
        /// Gets the linking URI of the file.
        /// </summary>
        public string LinkingUri { get; }

        /// <summary>
        /// Gets the linking URL of the file.
        /// </summary>
        public string LinkingUrl { get; }

        /// <summary>
        /// Gets a value that specifies the major version of the file.
        /// </summary>
        public int MajorVersion { get; }

        /// <summary>
        /// Gets a value that specifies the minor version of the file.
        /// </summary>
        public int MinorVersion { get; }

        /// <summary>
        /// Gets the name of the file including the extension.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the list page render type of the file.
        /// </summary>
        public ListPageRenderType PageRenderType { get; }

        /// <summary>
        /// Gets the relative URL of the file based on the URL for the server.
        /// </summary>
        public string ServerRelativeUrl { get; }

        /// <summary>
        /// Gets the Id of the Site collection in which the file is stored.
        /// </summary>
        public Guid SiteId { get; }

        /// <summary>
        ///	Gets a value that specifies when the file was created.
        /// </summary>
        public DateTime TimeCreated { get; }

        /// <summary>
        /// Gets a value that specifies when the file was last modified.
        /// </summary>
        public DateTime TimeLastModified { get; }

        /// <summary>
        /// Gets a value that specifies the display name of the file.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets a value that specifies the implementation-specific version identifier of the file.
        /// </summary>
        public int UIVersion { get; }

        /// <summary>
        /// Gets a value that specifies the implementation-specific version identifier of the file.
        /// </summary>
        public string UIVersionLabel { get; }

        /// <summary>
        /// Gets the unique Id of the file.
        /// </summary>
        public Guid UniqueId { get; }

        /// <summary>
        /// Gets the Id of the site in which the file is stored.
        /// </summary>
        public Guid WebId { get; }

        /// <summary>
        /// Gets a value that specifies the list item field values for the list item corresponding to the file.
        /// </summary>
        public IListItem ListItemAllFields { get; }

        //TODO: To implement...
        /// <summary>
        /// Gets a value that specifies the user who added the file.
        /// </summary>
        //public IUser Author { get; }

        //TODO: To implement...
        /// <summary>
        /// Gets a value that returns the user who has checked out the file.
        /// </summary>
        //public IUser CheckedOutByUser { get; }

        //TODO: To implement...
        /// <summary>
        /// To update...
        /// </summary>
        //public IEffectiveInformationRightsManagementSettings EffectiveInformationRightsManagementSettings { get; }

        //TODO: To implement...
        /// <summary>
        /// To update...
        /// </summary>
        //public IInformationRightsManagementFileSettings InformationRightsManagementSettings { get; }


        //TODO: To implement...
        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUser LockedByUser { get; }

        //TODO: To implement...
        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUser ModifiedBy { get; }

        //TODO: To implement...
        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IPropertyValues Properties { get; }

        //TODO: To implement...
        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IFileVersionEventCollection VersionEvents { get; }

        //TODO: To implement...
        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IFileVersionCollection Versions { get; }

        #region Publish
        /// <summary>
        /// Publish a major version of the current file.
        /// <paramref name="comment">The comments to add on file publishing.</paramref>
        /// </summary>
        Task PublishAsync(string comment = null);

        /// <summary>
        /// Publish a major version of the current file.
        /// <paramref name="comment">The comments to add on file publishing.</paramref>
        /// </summary>
        void Publish(string comment = null);

        /// <summary>
        /// Publish a major version of the current file.
        /// <paramref name="comment">The comments to add on file publishing.</paramref>
        /// </summary>
        Task PublishBatchAsync(string comment = null);

        /// <summary>
        /// Publish a major version of the current file.
        /// <paramref name="comment">The comments to add on file publishing.</paramref>
        /// </summary>
        void PublishBatch(string comment = null);


        /// <summary>
        /// Publish a major version of the current file.
        /// <paramref name="batch">The batch instance to use.</paramref>
        /// <paramref name="comment">The comments to add on file publishing.</paramref>
        /// </summary>
        Task PublishBatchAsync(Batch batch, string comment = null);

        /// <summary>
        /// Publish a major version of the current file.
        /// <paramref name="batch">The batch instance to use.</paramref>
        /// <paramref name="comment">The comments to add on file publishing.</paramref>
        /// </summary>
        void PublishBatch(Batch batch, string comment = null);
        #endregion

        #region Unpublish
        /// <summary>
        /// Unpublish the latest major version of the current file.
        /// <paramref name="comment">The comments to add on file unpublishing.</paramref>
        /// </summary>
        Task UnpublishAsync(string comment = null);

        /// <summary>
        /// Unpublish the latest major version of the current file.
        /// <paramref name="comment">The comments to add on file unpublishing.</paramref>
        /// </summary>
        void Unpublish(string comment = null);

        /// <summary>
        /// Unpublish the latest major version of the current file.
        /// <paramref name="comment">The comments to add on file unpublishing.</paramref>
        /// </summary>
        Task UnpublishBatchAsync(string comment = null);

        /// <summary>
        /// Unpublish the latest major version of the current file.
        /// <paramref name="comments">The comments to add on file unpublishing.</paramref>
        /// </summary>
        void UnpublishBatch(string comments = null);


        /// <summary>
        /// Unpublish the latest major version of the current file.
        /// <paramref name="batch">The batch instance to use.</paramref>
        /// <paramref name="comment">The comments to add on file unpublishing.</paramref>
        /// </summary>
        Task UnpublishBatchAsync(Batch batch, string comment = null);

        /// <summary>
        /// Unpublish the latest major version of the current file.
        /// <paramref name="batch">The batch instance to use.</paramref>
        /// <paramref name="comment">The comments to add on file unpublishing.</paramref>
        /// </summary>
        void UnpublishBatch(Batch batch, string comment = null);
        #endregion
    }
}
