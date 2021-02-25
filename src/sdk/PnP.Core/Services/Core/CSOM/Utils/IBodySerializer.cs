using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.Utils
{
    internal interface IBodySerializer
    {
        string SerializeRequestBody(List<ActionObjectPath> requests);
    }
}
