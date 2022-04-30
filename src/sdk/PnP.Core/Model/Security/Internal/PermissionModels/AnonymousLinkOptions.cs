using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Security
{
    internal class AnonymousLinkOptions : IAnonymousLinkOptions
    {
        public ShareType Type { get; set; }
        public string Password { get; set; }
        public DateTime ExpirationDateTime { get; set; }
    }
}
