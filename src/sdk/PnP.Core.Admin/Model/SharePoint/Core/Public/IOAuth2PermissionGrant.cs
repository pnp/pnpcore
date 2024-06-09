using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// An oauth2 permission granted on the SharePoint Online Client Extensibility Web Application Principal
    /// </summary>
    public interface IOAuth2PermissionGrant
    {
        /// <summary>
        /// ClientId of the SharePoint Online Client Extensibility Web Application Principal
        /// </summary>
        string ClientId { get; set; }
        /// <summary>
        /// Consent Type, e.g. AllPrincipals 
        /// </summary>
        string ConsentType { get; set; }
        /// <summary>
        /// Expires after
        /// </summary>
        DateTime ExpiryTime { get; set; }
        /// <summary>
        /// Grant Id
        /// </summary>
        string Id { get; set; }
        /// <summary>
        /// Id of the granted resource 
        /// </summary>
        string ResourceId { get; set; }
        /// <summary>
        /// Name of the granted resource
        /// </summary>
        string ResourceName { get; set; }
        /// <summary>
        /// List of scopes granted, separated by spaces, e.g. "Sites.Selected User.ReadBasic.All"
        /// </summary>
        string Scope { get; set; }
        /// <summary>
        /// Not valid before
        /// </summary>
        DateTime StartTime { get; set; }
    }
}