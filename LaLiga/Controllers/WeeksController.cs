using LaLiga.Context;
using LaLiga.DTOs;
using LaLiga.Entities;
using LaLiga.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaLiga.Controllers
{
    [ApiController]
    [Route("api/weeks")]
    public class WeeksController : ControllerBase
    {
        private readonly LaLigaContext _context;
        private readonly StandingsService _standings;

        public WeeksController(LaLigaContext context, StandingsService standings)
        {
            _context = context;
            _standings = standings;
        }

        // GET api/weeks  (hafta seçici: tarih aralığı, maç sayıları, güncel hafta, öne çıkan maç)
        [HttpGet]
        public async Task<ActionResult<List<WeekDto>>> GetWeeks()
        {
            var matches = await _context.Matches
                .AsNoTracking()
                .Select(m => new { m.Id, m.Week, m.MatchDate, m.Status, m.HomeTeamId, m.AwayTeamId })
                .ToListAsync();

            // Öne çıkan maç için güncel puan durumundaki sıralar
            var standings = await _standings.GetStandingsAsync();
            var position = standings.ToDictionary(s => s.TeamId, s => s.Position);
            int Rank(int teamId) => position.GetValueOrDefault(teamId, 99);

            var weeks = matches
                .GroupBy(m => m.Week)
                .OrderBy(g => g.Key)
                .Select(g => new WeekDto
                {
                    Week = g.Key,
                    StartDate = g.Min(m => m.MatchDate).Date,
                    EndDate = g.Max(m => m.MatchDate).Date,
                    MatchCount = g.Count(),
                    FinishedCount = g.Count(m => m.Status == MatchStatus.Finished),
                    LiveCount = g.Count(m => m.Status == MatchStatus.Live),

                    // Sıra toplamı en düşük çift; eşitlikte daha geç saatteki maç (prime-time)
                    FeaturedMatchId = g
                        .OrderBy(m => Rank(m.HomeTeamId) + Rank(m.AwayTeamId))
                        .ThenByDescending(m => m.MatchDate)
                        .ThenBy(m => m.Id)
                        .Select(m => (int?)m.Id)
                        .FirstOrDefault()
                })
                .ToList();

            // Güncel hafta: içinde başlamamış ya da canlı maç bulunan ilk hafta; yoksa son hafta
            var current = weeks.FirstOrDefault(w => matches.Any(m =>
                                  m.Week == w.Week &&
                                  (m.Status == MatchStatus.NotStarted || m.Status == MatchStatus.Live)))
                          ?? weeks.LastOrDefault();

            if (current != null)
                current.IsCurrent = true;

            return Ok(weeks);
        }
    }
}
