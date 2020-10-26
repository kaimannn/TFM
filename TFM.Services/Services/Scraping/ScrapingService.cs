using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TFM.Data.Models.Configuration;
using TFM.Data.Models.Metacritic;

namespace TFM.Services.Scraping
{
    public interface IScrapingService
    {
        Task<IEnumerable<MetacriticObject>> Scrape();
    }

    public class ScrapingService : IScrapingService
    {
        private readonly object _locker = new object();

        private readonly ILogger<ScrapingService> _logger = null;
        private readonly HttpClient _httpClient = null;
        private readonly AppSettings _config = null;

        public ScrapingService(AppSettings config, IHttpClientFactory clientFactory, ILogger<ScrapingService> logger)
        {
            _config = config;
            _logger = logger;
            _httpClient = clientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", _config.MetacriticApi.Host);
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", _config.MetacriticApi.Key);
        }

        public async Task<IEnumerable<MetacriticObject>> Scrape()
        {
            var metacriticObjects = new List<MetacriticObject>();

            var tasks = _config.Platforms.Select(platform =>
            {
                return Task.Run(async () =>
                {
                    int numPage = 0;
                    int numGames = 0;

                    // Get the first n games from metacritic
                    while (numGames < _config.NumGamesToRetrieve)
                    {
                        var scrapingUrl = string.Format(platform.Value.ScrapingUrl, numPage);

                        _logger.LogInformation($"Scraping: {scrapingUrl}");

                        // Get the Html Content from one metacritic web page
                        var htmlContent = await _httpClient.GetStringAsync(new Uri(scrapingUrl));

                        // Load the Html Content
                        var doc = new HtmlDocument();
                        doc.LoadHtml(htmlContent);

                        // Get the games titles
                        var titles = new List<string>();
                        foreach (var node in doc.DocumentNode.SelectNodes("//*[contains(@class,'clamp-image-wrap')]"))
                        {
                            var attribute = node.SelectSingleNode("a/img").Attributes["alt"];
                            if (attribute != null)
                                titles.Add(attribute.Value);
                        }

                        // get all the games info from metacritic
                        foreach (var title in titles)
                        {
                            try
                            {
                                var apiUrl = string.Format(platform.Value.ApiUrl, title);
                                var responseString = await _httpClient.GetStringAsync(new Uri(apiUrl));
                                var metacriticObject = JsonConvert.DeserializeObject<MetacriticObject>(responseString);
                                var imageBytes = await _httpClient.GetByteArrayAsync(new Uri(metacriticObject.Result.Image));

                                metacriticObject.Result.ImageBytes = imageBytes;
                                metacriticObject.Result.Platform = platform.Key;
                                metacriticObject.Result.Position = ++numGames;

                                lock (_locker)
                                    metacriticObjects.Add(metacriticObject);

                                _logger.LogInformation($"{numGames}. {platform.Key} game retrieved: {title}");

                                if (numGames == _config.NumGamesToRetrieve)
                                    break;
                            }
                            catch
                            {
                                _logger.LogInformation($"Failed to retrieve game: {title}");
                            }
                        }

                        numPage++;
                    }
                });
            });

            await Task.WhenAll(tasks);

            _logger.LogInformation($"Num. of scraped games: {metacriticObjects.Count}");

            await Task.Delay(500);

            return metacriticObjects;
        }
    }
}
