using LaLiga.WebUI.Services;
using LaLiga.WebUI.Settings;
using LaLiga.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LaLiga.WebUI.Controllers
{
    public class FixturesController : Controller
    {
        private readonly LaLigaApiClient _api;
        private readonly LeagueSettings _league;

        public FixturesController(LaLigaApiClient api, IOptions<LeagueSettings> league)
        {
            _api = api;
            _league = league.Value;
        }

        // GET /Fixtures  ve  /Fixtures?hafta=2
        public async Task<IActionResult> Index([FromQuery(Name = "hafta")] int? week)
        {
            var weeks = await _api.GetWeeksAsync();

            var currentWeek = weeks.FirstOrDefault(w => w.IsCurrent)?.Week ?? weeks.LastOrDefault()?.Week ?? 0;

            var selectedWeek = week.HasValue && weeks.Any(w => w.Week == week.Value) ? week.Value : currentWeek;

            var matches = selectedWeek == 0
                ? new List<MatchViewModel>()
                : await _api.GetMatchesByWeekAsync(selectedWeek);

            var model = new FixturesPageViewModel
            {
                Weeks = weeks,
                Matches = matches,
                LeagueName = _league.Name,
                Country = _league.Country,
                Season = _league.Season,
                TeamCount = _league.TeamCount,
                TotalWeeks = _league.TotalWeeks,
                CurrentWeek = currentWeek,
                SelectedWeek = selectedWeek
            };

            return View(model);
        }
    }
}
