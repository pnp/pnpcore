using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.Utils
{
    interface IBodySerializer
    {
        string SerializeRequestBody(List<ActionObjectPath> requests);
    }
}
