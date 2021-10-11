using System;
using System.Collections.Generic;

namespace PnP.Core.Admin.Model.Microsoft365
{
    internal class SensitivityLabel : ISensitivityLabel
    {
        public Guid Id {get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool IsDefault { get; set; }

        public List<string> ApplicableTo { get; set; }
    }
}
