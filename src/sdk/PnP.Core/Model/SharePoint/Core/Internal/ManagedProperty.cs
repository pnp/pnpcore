using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class ManagedProperty : IManagedProperty
    {
        public string Name { get; set; }

        public List<string> Aliases { get; set; }

        public List<string> Mappings { get; set; }

        public string Type { get; set; }

        public string Pid { get; set; }
    }
}
