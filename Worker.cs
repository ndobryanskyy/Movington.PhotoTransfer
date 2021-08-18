using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Movington.PhotoTransfer.Pipeline;

namespace Movington.PhotoTransfer
{
    public sealed class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IEnumerable<IInitializable> _initializables;
        private readonly TransferPipeline _transferPipeline;

        public Worker(
            ILogger<Worker> logger,
            IEnumerable<IInitializable> initializables,
            TransferPipeline transferPipeline)
        {
            _logger = logger;
            _initializables = initializables;
            _transferPipeline = transferPipeline;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var initializable in _initializables)
            {
                await initializable.InitializeAsync(stoppingToken);
            }

            _logger.LogInformation("Initialization completed");

            var sw = Stopwatch.StartNew();

            try
            {
                await _transferPipeline.ExecuteAsync(stoppingToken);
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation("Total pipeline runtime: {TotalPipelineRuntimeSec} seconds", sw.Elapsed.TotalSeconds);
            }
        }
    }
}