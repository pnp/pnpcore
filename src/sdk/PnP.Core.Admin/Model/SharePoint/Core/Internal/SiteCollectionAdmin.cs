using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal sealed class SiteCollectionAdmin : ISiteCollectionAdmin
    {
        public Guid Id { get; set; }

        public string LoginName { get; set; }

        public string Name { get; set; }

        public string UserPrincipalName { get; set; }

        public string Mail { get; set; }

        public bool IsSecondaryAdmin { get; set; }

        public bool IsMicrosoft365GroupOwner { get; set; }
    }
}
