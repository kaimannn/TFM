using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TFM.Data.Models.Configuration;

namespace TFM.Services.JobServices.Ping
{
    public interface IPingJobService
    {
    }

    public class PingJobService : BackgroundService, IPingJobService
    {
        private readonly ILogger<PingJobService> _logger = null;
        private readonly HttpClient _httpClient = null;
        private readonly AppSettings _config = null;

        public PingJobService(ILogger<PingJobService> logger, AppSettings config, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _config = config;
            _httpClient = clientFactory.CreateClient("httpClient");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PingJobService starts.");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(_config.Jobs.PingJobService.PingUrl, stoppingToken);

                        _logger.LogInformation($"Ping to {_config.Jobs.PingJobService.PingUrl} worked.");
                    }
                    catch
                    {
                        _logger.LogInformation($"Ping to {_config.Jobs.PingJobService.PingUrl} failed.");
                    }

                    await Task.Delay(TimeSpan.FromMinutes(_config.Jobs.PingJobService.FrequencyInMinutes), stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                _logger.LogInformation("PingJobService ends.");
            }
        }
    }
}
