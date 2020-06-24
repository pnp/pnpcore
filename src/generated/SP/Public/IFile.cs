using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a File object
    /// </summary>
    [ConcreteType(typeof(File))]
    public interface IFile : IDataModel<IFile>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string CheckInComment { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int CheckOutType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ContentTag { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int CustomizedPageStatus { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ListId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Exists { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IrmEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LinkingUri { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LinkingUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int MajorVersion { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int MinorVersion { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int PageRenderType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServerRelativeUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime TimeCreated { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime TimeLastModified { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int UIVersion { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string UIVersionLabel { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid UniqueId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid WebId { get; set; }

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

    }
}
