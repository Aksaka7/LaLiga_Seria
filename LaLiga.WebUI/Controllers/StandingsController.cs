using LaLiga.WebUI.Services;
using LaLiga.WebUI.Settings;
using LaLiga.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LaLiga.WebUI.Controllers
{
    public class StandingsController : Controller
    {
        private readonly LaLigaApiClient _api;
        private readonly LeagueSettings _league;

        public StandingsController(LaLigaApiClient api, IOptions<LeagueSettings> league)
        {
            _api = api;
            _league = league.Value;
        }

        // GET /  ve  /Standings
        public async Task<IActionResult> Index()
        {
            var standings = await _api.GetStandingsAsync();
            var weeks = await _api.GetWeeksAsync();

            var model = new StandingsPageViewModel
            {
                Standings = standings,
                LeagueName = _league.Name,
                Country = _league.Country,
                Season = _league.Season,
                CurrentWeek = weeks.FirstOrDefault(w => w.IsCurrent)?.Week ?? weeks.LastOrDefault()?.Week ?? 0,
                UpdatedAt = DateTime.Now
            };

            return View(model);
        }
    }
}
