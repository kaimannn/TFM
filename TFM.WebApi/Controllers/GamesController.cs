using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFM.Data.DB;
using TFM.Data.Models.Configuration;
using TFM.Data.Models.Ranking;

namespace TFM.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly TFMContext _db = null;
        private readonly AppSettings _config = null;

        public GamesController(TFMContext db, AppSettings config)
        {
            _db = db;
            _config = config;
        }

        [HttpGet]
        public async Task<IEnumerable<Game>> GetAll([FromQuery] Data.Filters.GamesFilter filter)
        {
            var query = _db.Games.AsQueryable();

            int numGamesToShow = _config.Ranking.DefaultNumGamesToShow;

            if (filter?.NumGamesToShow > 0)
                numGamesToShow = filter.NumGamesToShow;

            return await query
                .Where(g => g.Position <= numGamesToShow)
                .OrderBy(g => g.Position)
                .Select(g => new Game(g)).ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Game>> GetById(int id) =>
            await _db.Games.Where(g => g.Id == id).Select(g => new Game(g)).FirstOrDefaultAsync();
    }
}
