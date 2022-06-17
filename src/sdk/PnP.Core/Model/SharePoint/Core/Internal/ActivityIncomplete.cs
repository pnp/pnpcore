using System;

namespace PnP.Core.Model.SharePoint
{
    internal class ActivityIncomplete : IActivityIncomplete
    {
        public DateTime MissingDataBeforeDateTime { get; set; }

        public bool WasThrottled { get; set; }

        public bool ResultsPending { get; set; }

        public bool NotSupported { get; set; }
    }
}
