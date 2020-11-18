using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using TFM.Data.DB;
using TFM.Data.Models.Configuration;
using TFM.Data.Models.Ranking;

namespace TFM.Services.Ranking
{
    public interface IRankingService
    {
        Task<IEnumerable<MailMessage>> UpsertRankingAsync(IEnumerable<Game> games);
    }

    public class RankingService : IRankingService
    {
        private readonly object _locker = new object();

        private readonly AppSettings _config = null;
        private readonly IServiceProvider _sp = null;
        private readonly ILogger<RankingService> _logger;
        private readonly TranslationClient _translationClient;

        public RankingService(AppSettings config, IServiceProvider sp, ILogger<RankingService> logger)
        {
            _config = config;
            _sp = sp;
            _logger = logger;
            _translationClient = TranslationClient.Create(GoogleCredential.FromJson(JsonConvert.SerializeObject(_config.GoogleCredentials)));
        }

        public async Task<IEnumerable<MailMessage>> UpsertRankingAsync(IEnumerable<Game> games)
        {
            var mails = new List<MailMessage>();

            var tasks = _config.Metacritic.Platforms.Select(platform =>
            {
                return Task.Run(async () =>
                {
                    var scrapedGames = games.Where(g => g.Platform == platform.Key);

                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<TFMContext>();
                    var dbGames = await db.Games.Where(g => g.Platform == (int)platform.Key).ToListAsync();

                    // delete games
                    dbGames.Where(g => !scrapedGames.Any(sg => sg.Name == g.Name)).ToList().ForEach(g =>
                    {
                        g.Deleted = true;
                        g.ModifiedOn = DateTime.UtcNow;
                    });

                    foreach (var scrapedGame in scrapedGames)
                    {
                        var dbGame = dbGames.Where(g => g.Name == scrapedGame.Name).FirstOrDefault();
                        if (dbGame == null)
                        {
                            _ = db.Games.Add(new Games
                            {
                                CompanyName = scrapedGame.CompanyName,
                                LongDescription = (await _translationClient.TranslateTextAsync(scrapedGame.LongDescription, "es")).TranslatedText,
                                Name = scrapedGame.Name,
                                Position = scrapedGame.Position,
                                Score = scrapedGame.Score,
                                ReleaseDate = Convert.ToDateTime(scrapedGame.ReleaseDate),
                                CreatedOn = DateTime.UtcNow,
                                Platform = (int)scrapedGame.Platform,
                                ThumbnailUrl = scrapedGame.ThumbnailUrl,
                                Thumbnail = scrapedGame.ThumbnailBytes
                            });

                            if (dbGames.Count > 0)
                            {
                                lock (_locker)
                                    mails.Add(new MailMessage
                                    {
                                        Subject = "TODO: New Translation to Review!",
                                        Body = $"{scrapedGame.Name} -> New {scrapedGame.Platform} game inserted at position {scrapedGame.Position}"
                                    });
                            }
                        }
                        else
                        {
                            if (dbGame.Position != scrapedGame.Position || dbGame.Deleted)
                            {
                                dbGame.LastPosition = dbGame.Position;
                                dbGame.Position = scrapedGame.Position;
                                dbGame.ModifiedOn = DateTime.UtcNow;
                                dbGame.Deleted = false;
                            }
                        }
                    }

                    await db.SaveChangesAsync();

                    _logger.LogInformation($"{platform.Key} database updated!");
                });
            });

            await Task.WhenAll(tasks);

            return mails;
        }
    }
}
