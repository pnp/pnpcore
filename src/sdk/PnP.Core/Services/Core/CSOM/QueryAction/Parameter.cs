using PnP.Core.Services.Core.CSOM.QueryIdentities;
using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    internal class Parameter
    {
        internal string Type { get; set; }
        internal string Name { get; set; }
        internal object Value { get; set; }
    }

    internal class SelectQuery
    {
        internal bool SelectAllProperties { get; set; }
        internal List<Property> Properties { get; set; }

        public override string ToString()
        {
            return $"<Query SelectAllProperties=\"{SelectAllProperties.ToString().ToLower()}\"><Properties /></Query>";
        }
    }
}
