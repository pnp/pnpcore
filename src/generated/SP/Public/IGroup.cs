using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Group object
    /// </summary>
    [ConcreteType(typeof(Group))]
    public interface IGroup : IDataModel<IGroup>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowMembersEditMembership { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowRequestToJoinLeave { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AutoAcceptRequestToJoinLeave { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCurrentUserEditMembership { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCurrentUserManageGroup { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCurrentUserViewMembership { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool OnlyAllowMembersViewMembership { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string OwnerTitle { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string RequestToJoinLeaveEmailSetting { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IPrincipal Owner { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUserCollection Users { get; }

        #endregion

    }
}
