#if DEBUG
using System.Collections.Generic;

namespace PnP.Core.Services.Core
{
    internal class TestResponse
    {
        public bool IsSuccessStatusCode { get; set; }

        public int StatusCode { get; set; }

        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public string Response { get; set; }
    }
}
#endif