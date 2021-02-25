using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.QueryIdentities
{
    class StaticProperty : Identity
    {
        internal string TypeId { get; set; }
        public override string ToString()
        {
            return $"<StaticProperty Id=\"{Id}\" TypeId=\"{TypeId}\" Name=\"{Name}\" />";
        }
    }
}
