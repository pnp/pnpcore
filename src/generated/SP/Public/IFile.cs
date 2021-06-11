using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a File object
    /// </summary>
    [ConcreteType(typeof(File))]
    public interface IFile : IDataModel<IFile>, IDataModelGet<IFile>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public string CheckInComment { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int CheckOutType { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ContentTag { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int CustomizedPageStatus { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ListId { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ETag { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Exists { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IrmEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LinkingUri { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LinkingUrl { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int MajorVersion { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int MinorVersion { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int PageRenderType { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServerRelativeUrl { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid SiteId { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime TimeCreated { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime TimeLastModified { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int UIVersion { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string UIVersionLabel { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid UniqueId { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid WebId { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser Author { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser CheckedOutByUser { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IEffectiveInformationRightsManagementSettings EffectiveInformationRightsManagementSettings { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IInformationRightsManagementFileSettings InformationRightsManagementSettings { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IListItem ListItemAllFields { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser LockedByUser { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser ModifiedBy { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IPropertyValues Properties { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFileVersionEventCollection VersionEvents { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFileVersionCollection Versions { get; }

        #endregion

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool HasAlternateContentStreams { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServerRedirectedUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string VroomDriveID { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string VroomItemID { get; set; }

        #endregion

    }
}
