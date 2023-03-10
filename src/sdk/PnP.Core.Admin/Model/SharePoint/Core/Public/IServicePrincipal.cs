using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Manage the SharePoint apps service principal
    /// </summary>
    public interface IServicePrincipal
    {
        /// <summary>
        /// Approves the specified permission request
        /// </summary>
        /// <param name="id">permission request id</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>permission grant</returns>
        IPermissionGrant ApprovePermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Approves the specified permission request
        /// </summary>
        /// <param name="id">permission request id</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>permission grant</returns>
        Task<IPermissionGrant> ApprovePermissionRequestAsync(string id, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Denies the specified permission request
        /// </summary>
        /// <param name="id">permission request id</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        void DenyPermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Denies the specified permission request
        /// </summary>
        /// <param name="id">permission request id</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        Task DenyPermissionRequestAsync(string id, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Lists pending permission requests
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>the list of permission requests</returns>
        List<IPermissionRequest> GetPermissionRequests(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Lists pending permission requests
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>the list of permission requests</returns>
        Task<List<IPermissionRequest>> GetPermissionRequestsAsync(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Enable the Service Principal
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>the list of permission requests</returns>
        Task<IServicePrincipalProperties> EnableAsync(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Enable the Service Principal
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>the list of permission requests</returns>
        IServicePrincipalProperties Enable(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Disable the Service Principal
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>the list of permission requests</returns>
        Task<IServicePrincipalProperties> DisableAsync(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Disable the Service Principal
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>the list of permission requests</returns>
        IServicePrincipalProperties Disable(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Lists all granted Permissions
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>the list of granted permissions</returns>
        Task<IEnumerable<IPermissionGrant>> ListGrantsAsync(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Lists all granted Permissions
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>the list of granted permissions</returns>
        IEnumerable<IPermissionGrant> ListGrants(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Grants a Permission
        /// </summary>
        /// <param name="resource">Resource of the permission</param>
        /// <param name="scope">Scope of the permission</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>the new permission grant</returns>
        Task<IPermissionGrant> AddGrantAsync(string resource, string scope, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Grants a Permission
        /// </summary>
        /// <param name="resource">Resource of the permission</param>
        /// <param name="scope">Scope of the permission</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>the new permission grant</returns>
        IPermissionGrant AddGrant(string resource, string scope, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Revokes a Permission
        /// </summary>
        /// <param name="objectId">Object Id of the permission</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>the revoked permission grant</returns>
        Task<IPermissionGrant> RevokeGrantAsync(string objectId, VanityUrlOptions vanityUrlOptions = null);
        
        /// <summary>
        /// Revokes a Permission
        /// </summary>
        /// <param name="objectId">Object Id of the permission</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>the revoked permission grant</returns>
        IPermissionGrant RevokeGrant(string objectId, VanityUrlOptions vanityUrlOptions = null);
    }
}