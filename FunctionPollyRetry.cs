using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Polly;
using RestSharp;

namespace PollyExamples
{
    public class FunctionPollyRetry
    {
        private readonly ILogger _logger;

        private int[] httpStatusCodesWorthRetrying = { 404, 408, 500, 502, 503, 504 };

        public FunctionPollyRetry(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FunctionPollyRetry>();
        }

        [Function("Retry")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            //Wait and Retry policy after every retryAttempt number of seconds 3 times.
            //After that, the calls are stopped as the ExternalAPI keeps giving 500 error.

            var RetryPolicy = Policy.Handle<Exception>().WaitAndRetry(3,
                   attempt => TimeSpan.FromSeconds(0.1 * Math.Pow(2, attempt)),
                   (exception, calculatedWaitDuration) =>
                   {
                       _logger.LogError($"exception: {exception.Message}");
                   });
            try
            {
                RetryPolicy.Execute(() =>
                {
                    // Call a Webservice
                    var client = new RestClient("http://pslice.net/blahblah");
                    var request = new RestRequest();
                    RestResponse response = client.Execute(request);

                    // Force a retry
                    if (httpStatusCodesWorthRetrying.Contains((int)response.StatusCode))
                        throw new Exception("http request failed");

                    // Handle result
                    _logger.LogError($" result: {response.StatusCode}");
                });
            }
            catch (Exception e)
            {
                _logger.LogCritical($"critical error: {e.Message}");
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
