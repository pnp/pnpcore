using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    class QueryAction : BaseAction
    {
        internal SelectQuery SelectQuery { get; set; }
        public override string ToString()
        {
            return $"<Query Id=\"{Id}\" ObjectPathId=\"{ObjectPathId}\" >{SelectQuery.ToString()}</Query>";
        }
    }
}
