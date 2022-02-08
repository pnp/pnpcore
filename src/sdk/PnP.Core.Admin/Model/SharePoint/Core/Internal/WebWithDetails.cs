using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal sealed class WebWithDetails : IWebWithDetails
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ServerRelativeUrl { get; set; }

        public Language Language { get; set; }

        public Uri Url { get; set; }

        public string WebTemplate { get; set; }

        public string WebTemplateConfiguration { get; set; }

        public DateTime TimeCreated { get; set; }

        public DateTime LastItemModifiedDate { get; set; }

        public DateTime LastItemUserModifiedDate { get; set; }
    }
}
