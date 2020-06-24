using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a CreatablesInfo object
    /// </summary>
    [ConcreteType(typeof(CreatablesInfo))]
    public interface ICreatablesInfo : IDataModel<ICreatablesInfo>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCreateFolders { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCreateItems { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanUploadFiles { get; set; }

        #endregion

    }
}
