using LaLiga.DTOs;
using LaLiga.Services;
using Microsoft.AspNetCore.Mvc;

namespace LaLiga.Controllers
{
    [ApiController]
    [Route("api/standings")]
    public class StandingsController : ControllerBase
    {
        private readonly StandingsService _standings;

        public StandingsController(StandingsService standings)
        {
            _standings = standings;
        }

        // GET api/standings
        [HttpGet]
        public async Task<ActionResult<List<StandingDto>>> GetStandings()
        {
            return Ok(await _standings.GetStandingsAsync());
        }
    }
}
