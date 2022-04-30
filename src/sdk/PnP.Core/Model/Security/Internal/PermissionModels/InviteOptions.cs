using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Security
{
    internal class InviteOptions : IInviteOptions
    {
        public bool RequireSignIn { get; set; }
        public bool SendInvitation { get; set; }
        public string Message { get; set; }
        public List<IDriveRecipient> Recipients { get; set; }
        public List<PermissionRole> Roles { get; set; }
        public DateTime ExpirationDateTime { get; set; }
    }
}
