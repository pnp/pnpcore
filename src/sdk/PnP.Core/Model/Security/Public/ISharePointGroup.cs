namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a SharePoint Group
    /// </summary>
    [ConcreteType(typeof(SharePointGroup))]
    public interface ISharePointGroup : IDataModel<ISharePointGroup>, IDataModelGet<ISharePointGroup>, ISharePointPrincipal, IQueryableDataModel
    {
        /// <summary>
        /// Allow members to edit the group members
        /// </summary>
        public bool AllowMembersEditMembership { get; set; }

        /// <summary>
        /// Allow requests to join or leave the group
        /// </summary>
        public bool AllowRequestToJoinLeave { get; set; }

        /// <summary>
        /// Automatically accept requests to join or leave the group
        /// </summary>
        public bool AutoAcceptRequestToJoinLeave { get; set; }

        /// <summary>
        /// Can the current user edit the group members
        /// </summary>
        public bool CanCurrentUserEditMembership { get; set; }

        /// <summary>
        /// Can the current user manage the group
        /// </summary>
        public bool CanCurrentUserManageGroup { get; set; }

        /// <summary>
        /// Can the current user view the group membership
        /// </summary>
        public bool CanCurrentUserViewMembership { get; set; }

        /// <summary>
        /// Group description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Only members can see other group members
        /// </summary>
        public bool OnlyAllowMembersViewMembership { get; set; }

        /// <summary>
        /// Group owner title
        /// </summary>
        public string OwnerTitle { get; set; }

        /// <summary>
        /// Email configuration for the group join or leave operations
        /// </summary>
        public bool RequestToJoinLeaveEmailSetting { get; set; }
    }
}
