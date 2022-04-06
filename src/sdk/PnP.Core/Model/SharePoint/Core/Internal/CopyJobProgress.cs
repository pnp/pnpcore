using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class CopyJobProgress : ICopyJobProgress
    {
        public MigrationJobState JobState { get; set; }

        public IList<string> Logs { get; set; }

    }
}
