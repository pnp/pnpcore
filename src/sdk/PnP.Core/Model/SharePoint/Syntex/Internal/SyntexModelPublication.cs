using System;

namespace PnP.Core.Model.SharePoint
{

    internal class SyntexModelPublication : ISyntexModelPublication
    {

        public Guid ModelUniqueId { get; internal set; }

        public string TargetLibraryServerRelativeUrl { get; internal set; }

        public string TargetSiteUrl { get; internal set; }

        public string TargetWebServerRelativeUrl { get; internal set; }

        public MachineLearningPublicationViewOption ViewOption { get; internal set; }
    }
}
