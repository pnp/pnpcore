using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    internal class SetPropertyAction : BaseAction
    {
        internal string Name { get; set; }
        internal Parameter SetParameter { get; set; }

        public override string ToString()
        {
            string parameter = SetParameter?.SerializeParameter();
            return $"<SetProperty Id=\"{Id}\" ObjectPathId=\"{ObjectPathId}\" Name=\"{Name}\">{parameter}</SetProperty>";
        }
    }
}
