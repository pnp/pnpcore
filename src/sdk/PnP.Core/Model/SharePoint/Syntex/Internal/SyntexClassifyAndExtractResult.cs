using System;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class SyntexClassifyAndExtractResult : ISyntexClassifyAndExtractResult
    {
        public DateTime Created { get; internal set; }

        public DateTime DeliverDate { get; internal set; }

        public Guid Id { get; internal set; }

        public Guid WorkItemType { get; internal set; }

        public string ErrorMessage { get; internal set; }

        public int StatusCode { get; internal set; }

        public string Status { get; internal set; }

        public string TargetServerRelativeUrl { get; internal set; }

        public string TargetSiteUrl { get; internal set; }

        public string TargetWebServerRelativeUrl { get; internal set; }
    }
}
