using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Data.DB;

namespace WebApi.Services
{
    public interface IGamesService
    {
        Task<IEnumerable<Game>> GetGames(Data.Filters.GamesFilter filter);
    }

    public class GamesService : IGamesService
    {
        private readonly PSRankingContext _db = null;

        public GamesService(PSRankingContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Game>> GetGames(Data.Filters.GamesFilter filter)
        {
            var query = _db.Games.AsQueryable();

            if (filter?.MaxGames > 0)
                query = query.Where(g => g.Position <= filter.MaxGames);

            return await query.OrderBy(g => g.Position).ToListAsync();
        }
    }
}
