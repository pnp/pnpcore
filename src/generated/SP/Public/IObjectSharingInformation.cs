using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ObjectSharingInformation object
    /// </summary>
    [ConcreteType(typeof(ObjectSharingInformation))]
    public interface IObjectSharingInformation : IDataModel<IObjectSharingInformation>, IDataModelGet<IObjectSharingInformation>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string AnonymousEditLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string AnonymousViewLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanBeShared { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanBeUnshared { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanManagePermissions { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool HasPendingAccessRequests { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool HasPermissionLevels { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsSharedWithCurrentUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsSharedWithGuest { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsSharedWithMany { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsSharedWithSecurityGroup { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PendingAccessRequestsLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IObjectSharingInformationUserCollection SharedWithUsersCollection { get; }

        #endregion

    }
}
