using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TFM.Data.Models.Game;
using TFM.Services;

namespace TFM.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGamesService _gamesService;

        public GamesController(IGamesService gamesService)
        {
            _gamesService = gamesService;
        }

        [HttpGet]
        public async Task<IEnumerable<Game>> GetAll([FromQuery] Data.Filters.GamesFilter filter) => await _gamesService.GetAllAsync(filter);

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Game>> GetById(int id) => await _gamesService.GetByIdAsync(id);
    }
}
