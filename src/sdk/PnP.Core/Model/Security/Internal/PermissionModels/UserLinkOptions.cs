using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Security
{
    internal class UserLinkOptions : IUserLinkOptions
    {
        public ShareType Type { get; set; }
        public List<IDriveRecipient> Recipients { get; set; }
    }
}
