using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using RestSharp;

namespace PollyExamples
{
    public class FunctionPollyCircuitBreaker
    {
        private readonly ILogger _logger;

        private static int numFailures = 0;

        public FunctionPollyCircuitBreaker(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FunctionPollyCircuitBreaker>();
        }

        [Function("Circuit")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            var circuitBreaker = Policy
            .Handle<Exception>()
            .CircuitBreaker(3, TimeSpan.FromSeconds(5),
            onBreak: (ex, breakDelay) =>
            {
                Console.WriteLine($"Circuit breaker opened after {numFailures} failures.");
            },
            onReset: () =>
            {
                Console.WriteLine($"Circuit breaker reset.");
                numFailures = 0;
            });

            while (true)
            {
                circuitBreaker.Execute(() => MakePenzleCall());
            }
        }

        private static void MakePenzleCall()
        {
            if (numFailures < 3)
            {
                Console.WriteLine("Service call successful.");
                numFailures++;
            }
            else
            {
                Console.WriteLine("Service call failed.");
                throw new Exception("Service call failed.");
            }
        }
    }
}
