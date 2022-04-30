using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Represents a person, group, or other recipient to share a drive item with using the invite action.
    /// </summary>
    [ConcreteType(typeof(DriveRecipient))]
    public interface IDriveRecipient
    {
        /// <summary>
        /// The email address for the recipient, if the recipient has an associated email address.
        /// </summary>
        public string Email { get; set; }
    }
}
