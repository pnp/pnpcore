using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SharingPermissionInformation object
    /// </summary>
    [ConcreteType(typeof(SharingPermissionInformation))]
    public interface ISharingPermissionInformation : IDataModel<ISharingPermissionInformation>, IDataModelGet<ISharingPermissionInformation>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsDefaultPermission { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PermissionDescription { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PermissionId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int PermissionKind { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PermissionName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int PermissionRoleType { get; set; }

        #endregion

    }
}
