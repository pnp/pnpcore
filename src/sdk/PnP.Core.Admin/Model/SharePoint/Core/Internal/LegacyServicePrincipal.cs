using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal class LegacyServicePrincipal : ILegacyServicePrincipal
    {
        public Guid AppId { get; set; }

        public string AppIdentifier { get; set; }

        public string Name { get; set; }

        public DateTime ValidUntil { get; set; }
    }
}
