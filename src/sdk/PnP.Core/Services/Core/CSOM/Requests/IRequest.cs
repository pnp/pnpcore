using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.Requests
{
    internal interface IRequest<out T>
    {
        T Result { get; }

        List<ActionObjectPath> GetRequest(IIdProvider idProvider);

        void ProcessResponse(string response);
    }
}
