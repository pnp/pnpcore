using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    internal class BaseAction
    {
        public int Id { get; set; }
        public string ObjectPathId { get; set; }
        public override string ToString()
        {
            return $"<ObjectPath Id=\"{Id}\" ObjectPathId=\"{ObjectPathId}\" />";
        }
    }
}
