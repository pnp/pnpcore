using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Security
{
    [GraphType]
    internal sealed class DriveRecipient : IDriveRecipient
    {
        public string Email { get; set; }
    }
}
