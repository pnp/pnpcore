using System;
using System.Collections.Generic;

namespace PnP.Core.Model.Security
{

    /// <summary>
    /// Properties that can be set when creating a new Anonymous Link
    /// </summary>
    [ConcreteType(typeof(AnonymousLinkOptions))]
    public interface IAnonymousLinkOptions
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
        /// Indicates the expiration time of the permission.
        /// </summary>
        public DateTime ExpirationDateTime { get; set; }
    }
}
