using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM
{
    class ActionObjectPath
    {
        public BaseAction Action { get; set; }
        public Identity ObjectPath { get; set; }
    }
}
