using PnP.Core.Services.Core.CSOM.QueryIdentities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    class Parameter
    {
        internal string Type { get; set; }
        internal string Name { get; set; }
        internal string Value { get; set; }
    }
    class SelectQuery
    {
        internal bool SelectAllProperties { get; set; }
        internal List<Property> Properties { get; set; }

        public override string ToString()
        {
            return $"<Query SelectAllProperties=\"{SelectAllProperties.ToString().ToLower()}\"><Properties /></Query>";
        }
    }
}
