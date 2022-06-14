using System.Runtime.Serialization;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Link types
    /// </summary>
    public enum ShareType
    {
        /// <summary>
        /// Creates a read-only link to the driveItem.
        /// </summary>
        [EnumMember(Value="view")]
        View,
        /// <summary>
        /// Creates a review link to the driveItem. 
        /// Note: This option is only available for files in OneDrive for Business and SharePoint.
        /// </summary>
        [EnumMember(Value= "review")]
        Review,
        /// <summary>
        /// Creates an read-write link to the driveItem. 
        /// Note: This option is only available for files in OneDrive for Business and SharePoint.
        /// </summary>
        [EnumMember(Value = "edit")] 
        Edit,
        /// <summary>
        /// Creates an embeddable link to the item.
        /// </summary>
        [EnumMember(Value = "embed")]
        Embed,
        /// <summary>
        /// Creates a read-only link that blocks download to the driveItem. 
        /// Note: This option is only available for files in OneDrive for Business and SharePoint.
        /// </summary>
        [EnumMember(Value = "blocksDownload")] 
        BlocksDownload,
        /// <summary>
        /// Creates an upload-only link to the driveItem. 
        /// Note: This option is only available for folders in OneDrive for Business and SharePoint.
        /// </summary>
        [EnumMember(Value = "createOnly")] 
        CreateOnly,
        /// <summary>
        /// Creates the default link that is shown in the browser address bars for newly created files. 
        /// Note: Only available in OneDrive for Business and SharePoint. The organization admin configures whether this link type is supported, and what features are supported by this link type.
        /// </summary>
        [EnumMember(Value = "addressBar")] 
        AddressBar,
        /// <summary>
        /// Creates the default link to the driveItem as determined by the administrator of the organization. 
        /// Note: Only available in OneDrive for Business and SharePoint. The policy is enforced for the organization by the admin
        /// </summary>
        [EnumMember(Value = "adminDefault")] 
        AdminDefault
    }
}
