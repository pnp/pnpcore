using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal class SiteCollection : ISiteCollection
    {
        public Uri Url { get; set; }

        public Guid Id { get; set; }

        public string GraphId { get; set; }

        public Guid RootWebId { get; set; }

        public string Name { get; set; }
    }
}
