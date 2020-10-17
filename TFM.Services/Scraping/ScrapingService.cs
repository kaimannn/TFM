using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

        private readonly HttpClient _httpClient = null;
        private readonly Data.Models.Configuration.AppSettings _config = null;

        public ScrapingService(Data.Models.Configuration.AppSettings config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
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

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"Scraping: {scrapingUrl}");

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

                                lock (_locker) metacriticObjects.Add(metacriticObject);

                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine($"\t{numGames}. {platform.Key} game retrieved: {title}");

                                if (numGames == _config.NumGamesToRetrieve) break;
                            }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine($"\tFailed to retrieve game: {title}");
                            }
                        }

                        numPage++;
                    }
                });
            });

            await Task.WhenAll(tasks);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Num. of scraped games: {metacriticObjects.Count}");

            await Task.Delay(1000);

            return metacriticObjects;
        }
    }
}
