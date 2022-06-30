using System.Collections.Generic;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Properties that can be set when creating a new User Link
    /// </summary>
    public class UserLinkOptions : InviteOptionsBase
    {
        /// <summary>
        /// The type of sharing link to create.
        /// </summary>
        public ShareType Type { get; set; }

        /// <summary>
        /// A collection of recipients who will receive access to the sharing link.
        /// </summary>
        public List<IDriveRecipient> Recipients { get; set; }
    }
}
