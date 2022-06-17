using System;

namespace PnP.Core.Model.SharePoint
{
    internal class ActivityStat : IActivityStat
    {
        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public bool IsTrending { get; set; }

        public IActivityActionStat Create { get; set; }

        public IActivityActionStat Delete { get; set; }

        public IActivityActionStat Edit { get; set; }

        public IActivityActionStat Move { get; set; }

        public IActivityActionStat Access { get; set; }

        public IActivityIncomplete IncompleteData { get; set; }
    }
}
