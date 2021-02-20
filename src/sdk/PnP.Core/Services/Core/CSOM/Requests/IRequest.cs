using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.Requests
{
    interface IRequest<out T>
    {
        T Result { get; }
        List<ActionObjectPath> GetRequest(IIdProvider idProvider);
        void ProcessResponse(string response);
    }
}
