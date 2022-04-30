using System;
using System.Collections.Generic;

namespace PnP.Core.Model.Security
{

    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(ShareInviteRequestOptions))]
    public interface IShareInviteRequestOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public bool RequireSignIn { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool SendInvitation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<IDriveRecipient> Recipients { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<PermissionRole> Roles { get; set; }
    }
}
