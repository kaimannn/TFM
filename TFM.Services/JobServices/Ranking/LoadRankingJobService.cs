using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using TFM.Data.Models.Configuration;
using TFM.Data.Models.Ranking;
using TFM.Services.Mail;
using TFM.Services.Ranking;
using TFM.Services.Scraping;

namespace TFM.Services.JobServices.Ranking
{
    public class LoadRankingJobService : BackgroundService
    {
        private readonly ILogger<LoadRankingJobService> _logger = null;
        private readonly IMailService _mailService = null;
        private readonly IRankingService _rankingService = null;
        private readonly IScrapingService _scrapingService = null;

        public LoadRankingJobService(ILogger<LoadRankingJobService> logger,
            IMailService mailService,
            IRankingService rankingService,
            IScrapingService scrapingService)
        {
            _logger = logger;
            _mailService = mailService;
            _rankingService = rankingService;
            _scrapingService = scrapingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LoadRankingJobService starts.");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // scrape metacritic
                        var metacriticObjects = await _scrapingService.Scrape();

                        // update db ranking
                        var mails = await _rankingService.UpsertRanking(metacriticObjects.Select(mo => new Game(mo.Result)));

                        // new added games notifications
                        foreach (var mail in mails)
                        {
                            await _mailService.SendMail(mail);
                        }

                        _logger.LogInformation($"LoadRankingJobService has been executed OK");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"LoadRankingJobService failed: {ex}");
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1)); //aki variable del cronconfig
                }
            }
            catch (TaskCanceledException)
            {
                await _mailService.SendMail(new MailMessage
                {
                    Subject = "LoadRankingJobService has been canceled.",
                    Body = $"Restart de WebAPi to get the ranking updated once per day."
                });
            }
            finally
            {
                _logger.LogInformation("LoadRankingJobService ends.");
            }
        }
    }
}
