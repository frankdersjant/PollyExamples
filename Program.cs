using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

//
var host = new HostBuilder()

    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s =>
    {
        s.Configure<LoggerFilterOptions>(options =>
        {
        });
    })
    .Build();

host.Run();


