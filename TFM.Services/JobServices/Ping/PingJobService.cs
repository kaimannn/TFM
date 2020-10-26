using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using TFM.Data.Models.Configuration;
using TFM.Services.Mail;

namespace TFM.Services.JobServices.Ping
{
    public class PingJobService : BackgroundService
    {
        private readonly ILogger<PingJobService> _logger = null;
        private readonly HttpClient _httpClient = null;
        private readonly AppSettings _config = null;
        private readonly IMailService _mailService = null;

        public PingJobService(ILogger<PingJobService> logger, AppSettings config, IHttpClientFactory clientFactory, IMailService mailService)
        {
            _logger = logger;
            _config = config;
            _mailService = mailService;
            _httpClient = clientFactory.CreateClient();
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
                        var response = await _httpClient.GetAsync(_config.Jobs.PingJobService.PingUrl);

                        _logger.LogInformation($"Ping to {_config.Jobs.PingJobService.PingUrl} worked.");
                    }
                    catch (Exception)
                    {
                        _logger.LogInformation($"Ping to {_config.Jobs.PingJobService.PingUrl} failed.");
                    }

                    await Task.Delay(TimeSpan.FromMinutes(_config.Jobs.PingJobService.FrequencyInMinutes));
                }
            }
            catch (TaskCanceledException)
            {
                await _mailService.SendMail(new MailMessage
                {
                    Subject = "PingJobService has been canceled.",
                    Body = $"Restart de WebAPi to get the ranking updated once per day."
                });
            }
            finally
            {
                _logger.LogInformation("PingJobService ends.");
            }
        }
    }
}
