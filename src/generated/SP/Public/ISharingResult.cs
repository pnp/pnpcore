using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SharingResult object
    /// </summary>
    [ConcreteType(typeof(SharingResult))]
    public interface ISharingResult : IDataModel<ISharingResult>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PermissionsPageRelativeUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IGroupCollection GroupsSharedWith { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IGroup GroupUsersAddedTo { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUserCollection UsersWithAccessRequests { get; }

        #endregion

    }
}
