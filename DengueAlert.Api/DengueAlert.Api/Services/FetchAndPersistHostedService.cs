using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DengueAlert.Api.Services
{
        public class FetchAndPersistHostedService : IHostedService
        {
            private readonly ILogger<FetchAndPersistHostedService> _logger;
            private readonly IServiceProvider _sp;

            public FetchAndPersistHostedService(IServiceProvider sp, ILogger<FetchAndPersistHostedService> logger)
            {
                _sp = sp;
                _logger = logger;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("FetchAndPersistHostedService starting - will import last 6 months.");
                try
                {
                    using var scope = _sp.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<AlertDengueService>();
                    await service.ImportLastMonthsAsync(6);
                    _logger.LogInformation("Initial import finished.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during initial import.");
                }
            }

            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }
}
