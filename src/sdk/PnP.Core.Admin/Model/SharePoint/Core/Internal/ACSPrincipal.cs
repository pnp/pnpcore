using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal class ACSPrincipal : LegacyPrincipal, IACSPrincipal
    {
        public Guid AppId { get; set; }

        public string RedirectUri { get; set; }

        public string[] AppDomains { get; set; }

        public DateTime ValidUntil { get; set; }
    }
}
