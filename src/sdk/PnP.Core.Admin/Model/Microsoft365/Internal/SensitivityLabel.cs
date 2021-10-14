using System;

namespace PnP.Core.Admin.Model.Microsoft365
{
    internal class SensitivityLabel : ISensitivityLabel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public string Tooltip { get; set; }

        public int Sensitivity { get; set; }
    }
}
