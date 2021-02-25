using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.QueryIdentities
{
    class Identity
    {
        internal int Id { get; set; }
        internal string Name { get; set; }
        public override string ToString()
        {
            return $"<Identity Id=\"{Id}\" Name=\"{Name}\" />";
        }
    }
}
