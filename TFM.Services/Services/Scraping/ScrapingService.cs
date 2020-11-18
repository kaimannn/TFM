using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TFM.Data.Models.Configuration;
using TFM.Data.Models.Metacritic;

namespace TFM.Services.Scraping
{
    public interface IScrapingService
    {
        Task<IEnumerable<MetacriticGame>> ScrapeAsync();
    }

    public class ScrapingService : IScrapingService
    {
        private readonly object _locker = new object();

        private readonly ILogger<ScrapingService> _logger = null;
        private readonly HttpClient _httpClient = null;
        private readonly AppSettings _config = null;

        public ScrapingService(AppSettings config, ILogger<ScrapingService> logger, IHttpClientFactory clientFactory)
        {
            _config = config;
            _logger = logger;
            _httpClient = clientFactory.CreateClient("httpClient");
        }

        public async Task<IEnumerable<MetacriticGame>> ScrapeAsync()
        {
            var metacriticGames = new List<MetacriticGame>();

            var tasks = _config.Metacritic.Platforms.Select(platform =>
            {
                return Task.Run(async () =>
                {
                    int numPage = 0;
                    int numGames = 0;

                    // Get the first n games from metacritic
                    while (numGames < _config.Metacritic.NumGamesToRetrieve)
                    {
                        var scrapingUrl = string.Format(platform.Value.ScrapingUrl, numPage);

                        _logger.LogInformation($"Scraping: {scrapingUrl}");

                        // Get the Html Content from one metacritic web page
                        var htmlContent = await _httpClient.GetStringAsync(new Uri(scrapingUrl));

                        // Load the Html Content
                        var doc = new HtmlDocument();
                        doc.LoadHtml(htmlContent);

                        // Get the games relative urls
                        var relativeUrls = new List<string>();
                        foreach (var node in doc.DocumentNode.SelectNodes("//*[contains(@class,'clamp-image-wrap')]"))
                        {
                            var relativeUrl = node.SelectNodes(".//a").First().GetAttributeValue("href", "");
                            if (relativeUrl != null)
                                relativeUrls.Add(relativeUrl);
                        }

                        // get all the games info from metacritic
                        foreach (var relativeUrl in relativeUrls)
                        {
                            try
                            {
                                var gameUrl = _config.Metacritic.BaseUrl + relativeUrl;
                                var responseString = await _httpClient.GetStringAsync(new Uri(gameUrl));
                                var metacriticGame = ParseGame(responseString);
                                metacriticGame.ImageBytes = await _httpClient.GetByteArrayAsync(new Uri(metacriticGame.Image));
                                metacriticGame.Platform = platform.Key;
                                metacriticGame.Position = ++numGames;

                                lock (_locker)
                                    metacriticGames.Add(metacriticGame);

                                _logger.LogInformation($"{numGames}. {platform.Key} game retrieved: {metacriticGame.Title}");
                            }
                            catch
                            {
                                _logger.LogInformation($"Failed to retrieve game: {relativeUrl}");
                            }

                            if (numGames == _config.Metacritic.NumGamesToRetrieve)
                                break;
                        }

                        numPage++;
                    }
                });
            });

            await Task.WhenAll(tasks);
            await Task.Delay(500);

            _logger.LogInformation($"Num. of scraped games: {metacriticGames.Count}");

            return metacriticGames;
        }

        private static MetacriticGame ParseGame(string htmlString)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlString);

            var game = new MetacriticGame
            {
                Title = doc.DocumentNode.SelectNodes(@"//div[@class='content_head product_content_head game_content_head']/div[@class='product_title']/a/h1").First().InnerText,
                Score = int.Parse(doc.DocumentNode.SelectNodes(@"//div[@class='metascore_w xlarge game positive']").First().InnerText),
                Image = doc.DocumentNode.SelectNodes(@"//div[@class='product_image large_image must_play']/img[@class='product_image large_image']").First().GetAttributeValue("src", "")
            };

            int i = 0;
            string s;
            List<string> sList;

            sList = doc.DocumentNode.SelectNodes(@"//div[@class='content_head product_content_head game_content_head']/div[@class='product_data']/ul/li")
                .Select(li => li.InnerText.Replace("\t", "").Replace("\n", ""))
                .ToList();

            s = RemoveMultipleSpaces(sList[i]).Trim();
            s = s["Publisher: ".Length..];
            game.Publisher = s.Split(',').Select(t => t.Trim()).ToArray();

            s = RemoveMultipleSpaces(sList[++i]).Trim();
            game.ReleaseDate = s["Release Date: ".Length..];

            for (i += 1; i < sList.Count; i++)
            {
                if (sList[i].Contains("Also On:"))
                {
                    s = RemoveMultipleSpaces(sList[i]).Trim();
                    s = s["Also On: ".Length..];
                    game.AlsoAvailableOn = s.Split(',').Select(t => t.Trim()).ToArray();

                    break;
                }
            }

            sList = doc.DocumentNode.SelectNodes(@"//div[@class='section product_details']/div[@class='details side_details']/ul/li")
                .Select(li => li.InnerText.Replace("\t", "").Replace("\n", ""))
                .ToList();

            i = 0;
            s = RemoveMultipleSpaces(sList[i]).Trim();
            game.Developer = s["Developer: ".Length..];

            s = RemoveMultipleSpaces(sList[++i]).Trim();
            s = s["Genre(s): ".Length..];
            game.Genre = s.Split(',').Select(t => t.Trim()).ToArray();

            s = RemoveMultipleSpaces(sList[++i]).Trim();
            game.NumberOfPlayers = s["# of players: ".Length..];

            for (i += 1; i < sList.Count; i++)
            {
                if (sList[i].Contains("Rating:"))
                {
                    s = RemoveMultipleSpaces(sList[i]).Trim();
                    game.Rating = s["Rating: ".Length..];

                    break;
                }
            }

            try
            {
                sList = doc.DocumentNode.SelectNodes(@"//div[@class='section product_details']/div[@class='details main_details']/ul/li")
                  .Select(li => li.InnerText.Replace("\t", "").Replace("\n", ""))
                  .ToList();

                i = 0;
                s = RemoveMultipleSpaces(sList[i]).Trim();
                game.Description = s.Contains("&hellip;") ?
                    s.Substring("Summary: ".Length, s.IndexOf("&hellip;") - "&hellip;".Length - 1) : s["Summary: ".Length..];
            }
            catch
            {
                game.Description = "";
            }

            return game;
        }

        private static string RemoveMultipleSpaces(string sentence)
        {
            return new Regex("[ ]{2,}", RegexOptions.None).Replace(sentence, " ");
        }
    }
}
