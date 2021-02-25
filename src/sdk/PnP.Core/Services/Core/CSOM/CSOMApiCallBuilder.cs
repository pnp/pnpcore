using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM
{
    class CSOMApiCallBuilder
    {
        public IIdProvider IdProvider { get; set; } = new IteratorIdProvider();
        public IBodySerializer RequestBodySerializer { get; set; } = new CSOMBodySerializer();
        protected ICollection<IRequest<object>> Requests { get; set; } = new List<IRequest<object>>();

        public void AddRequest<T>(IRequest<T> request)
        {
            Requests.Add((IRequest<object>)request);
        }

        public ApiCall BuildApiCall()
        {

            var requests = new List<ActionObjectPath>();
            foreach (IRequest<object> request in Requests)
            {
                var actionRequests = request.GetRequest(IdProvider);

                requests.AddRange(actionRequests);
            }


            string requestBody = RequestBodySerializer.SerializeRequestBody(requests);

            return new ApiCall(requestBody);
        }
        public void ProcessRawResponse(string rawResponse)
        {
            foreach (var csomRequest in Requests)
            {
                csomRequest.ProcessResponse(rawResponse);
            }
        }
    }
}
