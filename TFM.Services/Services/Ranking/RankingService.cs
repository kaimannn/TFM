using Google.Cloud.Translation.V2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using TFM.Data.DB;
using TFM.Data.Models.Ranking;

namespace TFM.Services.Ranking
{
    public interface IRankingService
    {
        Task<IEnumerable<MailMessage>> UpsertRanking(IEnumerable<Game> games);
    }

    public class RankingService : IRankingService
    {
        private readonly object _locker = new object();

        private readonly Data.Models.Configuration.AppSettings _config = null;
        private readonly IServiceProvider _sp = null;
        private readonly ILogger<RankingService> _logger;
        private readonly TranslationClient _translationClient;

        public RankingService(Data.Models.Configuration.AppSettings config, IServiceProvider sp, ILogger<RankingService> logger)
        {
            _config = config;
            _sp = sp;
            _logger = logger;
            _translationClient = TranslationClient.Create();
        }

        public async Task<IEnumerable<MailMessage>> UpsertRanking(IEnumerable<Game> games)
        {
            var mails = new List<MailMessage>();

            var tasks = _config.Platforms.Select(platform =>
            {
                return Task.Run(async () =>
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<TFMContext>();

                    var platformGames = games.Where(g => g.Platform == platform.Key);
                    var dbPlatformGames = await db.Games.Where(g => g.Platform == (int)platform.Key).ToListAsync();
                    var gamesToRemove = dbPlatformGames.Where(pg => !platformGames.Any(g => g.Name == pg.Name));

                    db.Games.RemoveRange(gamesToRemove);

                    foreach (var game in platformGames)
                    {
                        var dbGame = dbPlatformGames.Where(g => g.Name == game.Name).FirstOrDefault();
                        if (dbGame == null)
                        {
                            _ = db.Games.Add(new Games
                            {
                                CompanyName = game.CompanyName,
                                LongDescription = (await _translationClient.TranslateHtmlAsync(game.LongDescription, "es")).TranslatedText,
                                Name = game.Name,
                                Position = game.Position,
                                Score = game.Score,
                                ReleaseDate = Convert.ToDateTime(game.ReleaseDate),
                                CreatedOn = DateTime.UtcNow,
                                Platform = (int)game.Platform,
                                ThumbnailUrl = game.ThumbnailUrl,
                                Thumbnail = game.ThumbnailBytes
                            });

                            if (dbPlatformGames.Any())
                            {
                                lock (_locker)
                                    mails.Add(new MailMessage { Subject = "TODO: New Translation to Review!", Body = $"{game.Name} -> New {game.Platform} game inserted at position {game.Position}" });
                            }
                        }
                        else
                        {
                            if (dbGame.Position != game.Position)
                            {
                                dbGame.LastPosition = dbGame.Position;
                                dbGame.Position = game.Position;
                                dbGame.ModifiedOn = DateTime.UtcNow;
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
