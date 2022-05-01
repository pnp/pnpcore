using System;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Properties that can be set when creating a new Anonymous Link
    /// </summary>
    public class AnonymousLinkOptions
    {
        /// <summary>
        /// The type of sharing link to create.
        /// </summary>
        public ShareType Type { get; set; }

        /// <summary>
        /// The password of the sharing link that is set by the creator.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Indicates the expiration datetime of the permission.
        /// </summary>
        public DateTime ExpirationDateTime { get; set; }
    }
}
