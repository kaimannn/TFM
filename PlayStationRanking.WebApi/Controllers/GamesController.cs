using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data.DB;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGamesService _gameService;

        public GamesController(IGamesService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public async Task<IEnumerable<Game>> Get([FromQuery] Data.Filters.GamesFilter filter) => await _gameService.GetGames(filter);

        [HttpPost]
        public void Post()
        {
            // Validate Game

            // Insert Game
        }
    }
}
