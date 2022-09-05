using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace WebAppTestFunction
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

		[Function("HandleEvent")]
		public static HttpResponseData RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
				FunctionContext executionContext)
		{
			var logger = executionContext.GetLogger(nameof(Function1));
			logger.LogInformation("C# HTTP trigger function processed a request.");

			var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
			var token = query["validationtoken"];

			if (!string.IsNullOrEmpty(token))
			{
				var tokenResponse = req.CreateResponse(HttpStatusCode.OK);
				tokenResponse.Headers.Add("Content-Type", "text/plain");
				tokenResponse.WriteString(token);
				return tokenResponse;
			}

			var response = req.CreateResponse(HttpStatusCode.OK);
			response.Headers.Add("Content-Type", "text/plain");
			return response;
		}

		[Function("HandleEventNew")]
		public static HttpResponseData RunAsync2([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
			FunctionContext executionContext)
		{
			var logger = executionContext.GetLogger(nameof(Function1));
			logger.LogInformation("C# HTTP trigger function processed a request.");

			var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
			var token = query["validationtoken"];

			if (!string.IsNullOrEmpty(token))
			{
				var tokenResponse = req.CreateResponse(HttpStatusCode.OK);
				tokenResponse.Headers.Add("Content-Type", "text/plain");
				tokenResponse.WriteString(token);
				return tokenResponse;
			}

			var response = req.CreateResponse(HttpStatusCode.OK);
			response.Headers.Add("Content-Type", "text/plain");

			return response;
		}
	}
}
