using System;
using System.Collections.Generic;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Options that can be set when inviting users to an item
    /// </summary>
    public class InviteOptions : InviteOptionsBase
    {
        /// <summary>
        /// Specifies where the recipient of the invitation is required to sign-in to view the shared item
        /// </summary>
        public bool RequireSignIn { get; set; }

        /// <summary>
        /// Specifies if an email or post is generated (false) or if the permission is just created (true).
        /// </summary>
        public bool SendInvitation { get; set; }

        /// <summary>
        /// A plain text formatted message that is included in the sharing invitation. Maximum length 2000 characters.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// A collection of recipients who will receive access and the sharing invitation.
        /// </summary>
        public List<IDriveRecipient> Recipients { get; set; }

        /// <summary>
        /// Specify the roles that are be granted to the recipients of the sharing invitation.
        /// </summary>
        public List<PermissionRole> Roles { get; set; }

        /// <summary>
        /// Specify the DateTime after which the permission expires. 
        /// Note: Available on OneDrive for Business, SharePoint, and premium personal OneDrive accounts.
        /// </summary>
        public DateTime ExpirationDateTime { get; set; }
    }
}
