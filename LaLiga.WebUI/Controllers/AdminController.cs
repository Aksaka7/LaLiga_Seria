using System.Globalization;
using System.Text;
using LaLiga.WebUI.Services;
using LaLiga.WebUI.Settings;
using LaLiga.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LaLiga.WebUI.Controllers
{
    public class AdminController : Controller
    {
        private readonly LaLigaApiClient _api;
        private readonly LeagueSettings _league;

        public AdminController(LaLigaApiClient api, IOptions<LeagueSettings> league)
        {
            _api = api;
            _league = league.Value;
        }

        public async Task<IActionResult> Matches(string? team, int? week, MatchStatus? status)
        {
            var filtered = await _api.GetMatchesAsync(week, status, team);

            bool noFilter = week is null && status is null && string.IsNullOrWhiteSpace(team);
            var totalCount = noFilter ? filtered.Count : (await _api.GetMatchesAsync()).Count;

            var model = new AdminMatchesPageViewModel
            {
                Matches = filtered,
                TotalCount = totalCount,
                TotalWeeks = _league.TotalWeeks,
                Teams = await _api.GetTeamsAsync(),
                FilterTeam = team,
                FilterWeek = week,
                FilterStatus = status
            };

            return View(model);
        }

        [HttpPost("Admin/Matches/Save")]
        public async Task<IActionResult> Save(AdminMatchFormModel form)
        {
            if (!DateOnly.TryParse(form.Date, out var date) || !TimeOnly.TryParse(form.Time, out var time))
            {
                return BadRequest(new { ok = false, message = "Geçerli bir tarih ve saat girin." });
            }

            var input = new MatchInputModel
            {
                HomeTeamId = form.Home,
                AwayTeamId = form.Away,
                Week = form.Week,
                MatchDate = date.ToDateTime(time),
                Status = form.Status,
                Stadium = form.Stadium,
                HomeScore = form.HomeScore,
                AwayScore = form.AwayScore,
                Minute = form.Minute,
                Note = form.Note,
                Referee = form.Referee,
                Attendance = form.Attendance
            };

            try
            {
                if (form.Id.HasValue)
                {
                    await _api.UpdateMatchAsync(form.Id.Value, input);
                    return Json(new { ok = true, id = form.Id.Value });
                }

                var created = await _api.CreateMatchAsync(input);
                return Json(new { ok = true, id = created.Id });
            }
            catch (ApiException ex)
            {
                return StatusCode((int)ex.StatusCode, new { ok = false, message = ex.Message, errors = ex.Errors });
            }
        }

        [HttpPost("Admin/Matches/Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _api.DeleteMatchAsync(id);
                return Json(new { ok = true });
            }
            catch (ApiException ex)
            {
                return StatusCode((int)ex.StatusCode, new { ok = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Teams(string? q, TeamRegion? region, string? status, string? sort, string? dir)
        {
            var all = await _api.GetTeamsAsync();
            var activeTeams = all.Where(t => t.IsActive).ToList();

            var stadiumCapacity = activeTeams
                .GroupBy(t => t.Stadium)
                .ToDictionary(g => g.Key, g => g.First().Capacity);
            var cities = new HashSet<string>(activeTeams.Select(t => t.City));

            var filtered = all.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var needle = Normalize(q.Trim());
                filtered = filtered.Where(t =>
                    Normalize(t.Name).Contains(needle) ||
                    Normalize(t.City).Contains(needle) ||
                    Normalize(t.Stadium).Contains(needle) ||
                    Normalize(t.Code).Contains(needle));
            }

            if (region.HasValue)
                filtered = filtered.Where(t => t.Region == region.Value);

            if (status == "active")
                filtered = filtered.Where(t => t.IsActive);
            else if (status == "inactive")
                filtered = filtered.Where(t => !t.IsActive);

            var sortKey = sort is "name" or "capacity" or "founded" ? sort : "id";
            var sortDir = dir == "desc" ? "desc" : "asc";

            var ordered = (sortKey, sortDir) switch
            {
                ("name", "desc") => filtered.OrderByDescending(t => t.Name, StringComparer.Create(new CultureInfo("tr-TR"), false)),
                ("name", _) => filtered.OrderBy(t => t.Name, StringComparer.Create(new CultureInfo("tr-TR"), false)),
                ("capacity", "desc") => filtered.OrderByDescending(t => t.Capacity),
                ("capacity", _) => filtered.OrderBy(t => t.Capacity),
                ("founded", "desc") => filtered.OrderByDescending(t => t.FoundedYear),
                ("founded", _) => filtered.OrderBy(t => t.FoundedYear),
                (_, "desc") => filtered.OrderByDescending(t => t.Id),
                _ => filtered.OrderBy(t => t.Id)
            };

            var model = new AdminTeamsPageViewModel
            {
                Teams = ordered.ToList(),
                TotalCount = all.Count,
                MaxActive = _league.MaxActiveTeams,
                ActiveCount = activeTeams.Count,
                CityCount = cities.Count,
                TotalCapacity = stadiumCapacity.Values.Sum(),
                Oldest = activeTeams.OrderBy(t => t.FoundedYear).FirstOrDefault(),
                FilterQuery = q,
                FilterRegion = region,
                FilterStatus = status,
                SortKey = sortKey,
                SortDir = sortDir
            };

            return View(model);
        }

        // POST /Admin/Teams/Save  (ekleme: form.Id boş; güncelleme: form.Id dolu)
        [HttpPost("Admin/Teams/Save")]
        public async Task<IActionResult> SaveTeam(AdminTeamFormModel form)
        {
            var input = new TeamInputModel
            {
                Name = form.Name,
                Code = form.Code,
                City = form.City,
                Region = form.Region,
                Stadium = form.Stadium,
                Capacity = form.Capacity,
                FoundedYear = form.Founded,
                ColorA = form.ColorA,
                ColorB = form.ColorB,
                IsActive = form.Status == "active"
            };

            try
            {
                if (form.Id.HasValue)
                {
                    await _api.UpdateTeamAsync(form.Id.Value, input);
                    return Json(new { ok = true, id = form.Id.Value });
                }

                var created = await _api.CreateTeamAsync(input);
                return Json(new { ok = true, id = created.Id });
            }
            catch (ApiException ex)
            {
                return StatusCode((int)ex.StatusCode, new { ok = false, message = ex.Message, errors = ex.Errors });
            }
        }

        // POST /Admin/Teams/Delete/5  (API 409 döner: maçı olan takım silinemez)
        [HttpPost("Admin/Teams/Delete/{id:int}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            try
            {
                await _api.DeleteTeamAsync(id);
                return Json(new { ok = true });
            }
            catch (ApiException ex)
            {
                return StatusCode((int)ex.StatusCode, new { ok = false, message = ex.Message });
            }
        }

        // Aksan ve harf duyarsız arama için (API'deki MatchesController.Normalize ile aynı mantık)
        private static string Normalize(string value)
        {
            var decomposed = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var chars = decomposed.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
            return new string(chars.ToArray());
        }
    }
}
