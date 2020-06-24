using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a MountedFolderInfo object
    /// </summary>
    [ConcreteType(typeof(MountedFolderInfo))]
    public interface IMountedFolderInfo : IDataModel<IMountedFolderInfo>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string FolderUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool HasEditPermission { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ListTemplateType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ListViewUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string WebUrl { get; set; }

        #endregion

    }
}
