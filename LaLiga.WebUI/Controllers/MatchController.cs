using LaLiga.WebUI.Services;
using LaLiga.WebUI.Settings;
using LaLiga.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LaLiga.WebUI.Controllers
{
    public class MatchController : Controller
    {
        private readonly LaLigaApiClient _api;
        private readonly LeagueSettings _league;

        public MatchController(LaLigaApiClient api, IOptions<LeagueSettings> league)
        {
            _api = api;
            _league = league.Value;
        }

        // GET /Match/Detail/11
        public async Task<IActionResult> Detail(int id)
        {
            // Maç yoksa API 404 döner
            var match = await _api.GetMatchAsync(id);

            var model = new MatchDetailPageViewModel
            {
                Match = match,
                LeagueName = _league.Name,
                Country = _league.Country,
                Season = _league.Season
            };

            return View(model);
        }
    }
}
