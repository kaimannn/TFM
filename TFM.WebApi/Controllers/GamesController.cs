using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TFM.Data.DB;
using TFM.Data.Models.Ranking;

namespace TFM.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly TFMContext _db = null;

        public GamesController(TFMContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IEnumerable<Game>> GetAll([FromQuery] Data.Filters.GamesFilter filter)
        {
            var query = _db.Games.AsQueryable();

            if (filter?.NumberOfGames > 0)
                query = query.Where(g => g.Position <= filter.NumberOfGames);

            return await query.OrderBy(g => g.Position).Select(g => new Game(g)).ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Game>> GetById(int id) =>
            await _db.Games.Where(g => g.Id == id).Select(g => new Game(g)).FirstOrDefaultAsync(); 
    }
}
