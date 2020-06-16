using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Test.Services
{
    internal class MockResponseHandler: HttpMessageHandler
    {
        private HttpResponseMessage Response1 { get; set; }
        private HttpResponseMessage Response2 { get; set; }

        private bool response1Sent = false;

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!response1Sent)
            {
                response1Sent = true;
                Response1.RequestMessage = request;
                return await Task.FromResult(Response1);
            }
            else
            {
                response1Sent = false;
                Response2.RequestMessage = request;
                return await Task.FromResult(Response2);
            }
        }

        public void SetHttpResponse(HttpResponseMessage response1, HttpResponseMessage response2 = null)
        {
            this.response1Sent = false;
            this.Response1 = response1;
            this.Response2 = response2;
        }
    }
}
