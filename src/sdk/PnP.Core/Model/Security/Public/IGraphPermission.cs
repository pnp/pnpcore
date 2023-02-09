using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// The permission resource provides information about a sharing permission granted for a driveItem resource.
    /// Sharing permissions have a number of different forms. The permission resource represents these different forms through facets on the resource.
    /// Note: OneDrive for Business and SharePoint document libraries do not return the inheritedFrom property.
    ///       grantedTo and grantedToIdentities will be deprecated going forward and the response will be migrated to grantedToV2 and grantedToIdentitiesV2 respectively under 
    ///       appropriate property names.
    /// </summary>
    [ConcreteType(typeof(GraphPermission))]
    public interface IGraphPermission : IDataModel<IGraphPermission>
    {
        #region Properties

        /// <summary>
        /// The unique identifier of the permission among all permissions on the item. 
        /// Note: Read-only.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// For user type permissions, the details of the users and applications for this permission. 
        /// Note: Read-only.
        /// </summary>
        ISharePointIdentitySet GrantedToV2 { get; }

        /// <summary>
        /// For link type permissions, the details of the users to whom permission was granted.
        /// Note: Read-only.
        /// </summary>
        List<ISharePointIdentitySet> GrantedToIdentitiesV2 { get; }

        /// <summary>
        /// Details of any associated sharing invitation for this permission. 
        /// Note: Read-only.
        /// </summary>
        ISharingInvitation Invitation { get; }

        /// <summary>
        /// Provides the link details of the current permission, if it is a link type permissions. 
        /// Note: Read-only.
        /// </summary>
        ISharingLink Link { get; }

        /// <summary>
        /// The type of permission
        /// </summary>
        List<PermissionRole> Roles { get; }

        /// <summary>
        /// A unique token that can be used to access this shared item via the shares API. 
        /// Note: Read-only.
        /// </summary>
        string ShareId { get; }

        /// <summary>
        /// A format of yyyy-MM-ddTHH:mm:ssZ of DateTimeOffset indicates the expiration time of the permission. DateTime.MinValue indicates there is no expiration set for this permission. Optional.
        /// </summary>
        DateTime ExpirationDateTime { get; }

        /// <summary>
        /// Indicates whether the password is set for this permission. 
        /// Note: This property only appears in the response. Optional. Read-only. For OneDrive Personal only.
        /// </summary>
        bool HasPassword { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds permissions for a set of users to a specific permission
        /// </summary>
        /// <param name="recipients">List of recipients to add</param>
        /// <returns></returns>
        Task<IGraphPermission> GrantUserPermissionsAsync(List<IDriveRecipient> recipients);

        /// <summary>
        /// Adds permissions for a set of users to a specific permission
        /// </summary>
        /// <param name="recipients">List of recipients to add</param>
        /// <returns></returns>
        IGraphPermission GrantUserPermissions(List<IDriveRecipient> recipients);

        /// <summary>
        /// Removes permissions for a set of users from a specific permission
        /// </summary>
        /// <param name="recipients">List of recipients to remove</param>
        /// <returns></returns>
        Task<IGraphPermission> RemoveUserPermissionsAsync(List<IDriveRecipient> recipients);

        /// <summary>
        /// Removes permissions for a set of users from a specific permission
        /// </summary>
        /// <param name="recipients">List of recipients to remove</param>
        /// <returns></returns>
        IGraphPermission RemoveUserPermissions(List<IDriveRecipient> recipients);

        /// <summary>
        /// Delete a specific permission
        /// </summary>
        Task DeletePermissionAsync();

        /// <summary>
        /// Delete a specific permission
        /// </summary>
        void DeletePermission();

        #endregion
    }
}
