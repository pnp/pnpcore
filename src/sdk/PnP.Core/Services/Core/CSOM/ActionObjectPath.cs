using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;

namespace PnP.Core.Services.Core.CSOM
{
    internal class ActionObjectPath
    {
        internal BaseAction Action { get; set; }

        internal Identity ObjectPath { get; set; }
    }
}
