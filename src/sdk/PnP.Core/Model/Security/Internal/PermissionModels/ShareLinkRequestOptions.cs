using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Security
{
    internal class ShareLinkRequestOptions : IShareLinkRequestOptions
    {
        public ShareType Type { get; set; }
        public DateTime ExpirationDateTime { get; set; }
        public ShareScope Scope { get; set; }
    }
}
