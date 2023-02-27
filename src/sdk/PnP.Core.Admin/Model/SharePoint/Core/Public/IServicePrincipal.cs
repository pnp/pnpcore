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
        Task<IServicePrincipalProperties> Enable(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Disable the Service Principal
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>the list of permission requests</returns>
        Task<IServicePrincipalProperties> Disable(VanityUrlOptions vanityUrlOptions = null);
    }
}
