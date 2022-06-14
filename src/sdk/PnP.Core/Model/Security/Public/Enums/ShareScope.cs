using System.Runtime.Serialization;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// The scope of a link
    /// </summary>
    public enum ShareScope
    {
        /// <summary>
        /// Anyone with the link has access, without needing to sign in. This may include people outside of your organization. 
        /// Anonymous link support may be disabled by an administrator.
        /// </summary>
        [EnumMember(Value = "anonymous")]
        Anonymous,
        
        /// <summary>
        /// Anyone signed into your organization (tenant) can use the link to get access. Only available in OneDrive for Business and SharePoint.
        /// </summary>
        [EnumMember(Value = "organization")]
        Organization,

        /// <summary>
        /// Specific people in the recipients collection can use the link to get access. Only available in OneDrive for Business and SharePoint.
        /// </summary>
        [EnumMember(Value = "users")]
        Users
    }
}
