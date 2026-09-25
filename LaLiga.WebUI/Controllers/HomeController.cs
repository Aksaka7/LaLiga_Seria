using System.Diagnostics;
using LaLiga.WebUI.Services;
using LaLiga.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LaLiga.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly LaLigaApiClient _api;

        public HomeController(LaLigaApiClient api)
        {
            _api = api;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var standings = await _api.GetStandingsAsync();
                var weeks = await _api.GetWeeksAsync();
                var matches = await _api.GetMatchesAsync();

                return Content($"API bağlantısı OK | takım: {standings.Count} | hafta: {weeks.Count} | maç: {matches.Count} | lider: {standings[0].TeamName}");
            }
            catch (ApiException ex)
            {
                return Content($"API hatası ({(int)ex.StatusCode}): {ex.Message}");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
