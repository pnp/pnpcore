using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services
{
    public class CustomHeadersRequestModule : RequestModuleBase
    {
        public CustomHeadersRequestModule(Dictionary<string, string> headers)
        {
            Headers = headers;

            RequestHeaderHandler = (Dictionary<string, string> headers) =>
            {
                headers.Add("Bert", "Jansen");
            };
        }

        public override Guid Id { get => Guid.Parse("{46307280-190E-4365-8AA1-085C451E7799}"); }

        public Dictionary<string, string> Headers { get; private set; }

    }
}
