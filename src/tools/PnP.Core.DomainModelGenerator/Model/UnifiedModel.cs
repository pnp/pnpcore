using System.Collections.Generic;

namespace PnP.M365.DomainModelGenerator
{
    internal class UnifiedModel
    {
        public string Provider { get; set; }

        public List<UnifiedModelEntity> Entities { get; set; } = new List<UnifiedModelEntity>();
    }
}
