using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ObjectSharingInformationUser object
    /// </summary>
    [ConcreteType(typeof(ObjectSharingInformationUser))]
    public interface IObjectSharingInformationUser : IDataModel<IObjectSharingInformationUser>, IDataModelGet<IObjectSharingInformationUser>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string CustomRoleNames { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool HasEditPermission { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool HasReviewPermission { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool HasViewPermission { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsDomainGroup { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsExternalUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsMemberOfGroup { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsSiteAdmin { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string JobTitle { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SipAddress { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IPrincipal Principal { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser User { get; }

        #endregion

    }
}
