using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using TFM.Data.DB;
using TFM.Data.Filters;
using TFM.Data.Models.Game;

namespace TFM.Services
{
    public interface IGamesService
    {
        Task<IEnumerable<Game>> GetAllAsync(GamesFilter filter);
        Task<Game> GetByIdAsync(int id);
        Task<IEnumerable<MailMessage>> UpsertRanking(IEnumerable<Game> games);
    }

    public class GamesService : IGamesService
    {
        private readonly object _locker = new object();

        private readonly TFMContext _db = null;
        private readonly Data.Models.Configuration.AppSettings _config = null;
        private readonly IServiceProvider _sp = null;

        public GamesService(TFMContext db, Data.Models.Configuration.AppSettings config, IServiceProvider sp)
        {
            _db = db;
            _config = config;
            _sp = sp;
        }

        public async Task<Game> GetByIdAsync(int id) => await _db.Games.Where(g => g.Id == id).Select(g => new Game(g)).FirstOrDefaultAsync();

        public async Task<IEnumerable<Game>> GetAllAsync(GamesFilter filter)
        {
            var query = _db.Games.AsQueryable();

            if (filter?.MaxGames > 0)
                query = query.Where(g => g.Position <= filter.MaxGames);

            return await query.OrderBy(g => g.Position).Select(g => new Game(g)).ToListAsync();
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
                                LongDescription = game.LongDescription,
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
                                    mails.Add(new MailMessage { Subject = "TODO: New Translation!", Body = $"{ game.Name} -> New { game.Platform} game inserted at position { game.Position}" });
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

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{platform.Key} database updated!");
                });
            });

            await Task.WhenAll(tasks);

            return mails;
        }
    }
}
