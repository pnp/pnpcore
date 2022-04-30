using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(DriveRecipient))]
    public interface IDriveRecipient
    {
        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; }
    }
}
