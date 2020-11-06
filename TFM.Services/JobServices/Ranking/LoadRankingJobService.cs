using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
        private readonly AppSettings _config = null;

        public LoadRankingJobService(ILogger<LoadRankingJobService> logger,
            IMailService mailService,
            IRankingService rankingService,
            IScrapingService scrapingService,
            AppSettings config)
        {
            _logger = logger;
            _mailService = mailService;
            _rankingService = rankingService;
            _scrapingService = scrapingService;
            _config = config;
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
                        var metacriticGames =
                            await _scrapingService.ScrapeAsync();
                        if (metacriticGames.Count() < _config.Metacritic.NumGamesToRetrieve)
                            throw new Exception($"Not enough games retrieved. Number of games retrieved is {metacriticGames.Count()} and should be retrieved {_config.Metacritic.NumGamesToRetrieve}");

                        // update db ranking
                        var mails = await _rankingService.UpsertRankingAsync(metacriticGames.Select(mg => new Game(mg)));

                        // new added games notifications
                        foreach (var mail in mails)
                            await _mailService.SendMailAsync(mail);

                        _logger.LogInformation($"LoadRankingJobService has been executed OK");
                    }
                    catch (Exception ex)
                    {
                        using (var msg = new MailMessage
                        {
                            Subject = "Something went wrong!",
                            Body = ex.ToString()
                        })
                        {
                            await _mailService.SendMailAsync(msg);
                        }

                        _logger.LogInformation($"LoadRankingJobService failed: {ex}");
                    }

                    // Calculate delay
                    var now = DateTime.Now;
                    var tomorrow = now.AddDays(1);
                    var delay = new DateTime(
                        tomorrow.Year,
                        tomorrow.Month,
                        tomorrow.Day,
                        _config.Jobs.LoadRankingJobService.ExecutionHour,
                        _config.Jobs.LoadRankingJobService.ExecutionMinute,
                        0) - now;

                    await Task.Delay(delay, stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                _logger.LogInformation("LoadRankingJobService ends.");
            }
        }
    }
}
