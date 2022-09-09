using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Redis.OM;
using WatchLogger.Helpers;

namespace WatchLogger.HostedService
{
   
    public class IndexCreationService : BackgroundService
    {
        private readonly RedisConnectionProvider _provider;
        private readonly IServiceProvider _serviceProvider;
        private ILogger<IndexCreationService> _logger;

        public IndexCreationService(
            RedisConnectionProvider provider, 
            IServiceProvider serviceProvider, 
            ILogger<IndexCreationService> logger)
        {
           // _provider = new RedisConnectionProvider(WatchLogExternalDbConfig.ConnectionString);

            _provider = provider;
            _serviceProvider = serviceProvider;
            _logger =  logger;
        }

        /// <summary>
        /// Checks redis to see if the index already exists, if it doesn't create a new index
        /// </summary>
   
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    //var loggerService = scope.ServiceProvider.GetService<ILogger>();
                    try
                    {
                        _logger.LogInformation("Index Creation Background service is starting");
                        _logger.LogInformation($"Index is creating...");

                        //var info = (await _provider.Connection.ExecuteAsync("FT._LIST")).ToArray().Select(x => x.ToString());
                        //if (info.All(x => x != "rediswatchlog-idx"))
                        //{
                        //    await _provider.Connection.CreateIndexAsync(typeof(WLog));
                        //    _logger.LogInformation($"Index creation Dones...");
                        //}
                        await _provider.Connection.CreateIndexAsync(typeof(WLog));
                        _logger.LogInformation($"Index Creation Done...");


                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);

                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Log Clearer Background service error : {ex.Message}");
            }


    
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
