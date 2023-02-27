using System.Collections.Generic;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal class ServicePrincipalProperties: IServicePrincipalProperties
    {
        public bool AccountEnabled { get; set; }

        public string AppId { get; set; }

        public IEnumerable<string> ReplyUrls { get; set; }
    }
}
