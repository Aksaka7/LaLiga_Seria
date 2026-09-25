using LaLiga.Context;
using LaLiga.DTOs;
using LaLiga.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaLiga.Services
{
    // Puan durumu ayrı bir tabloda tutulmaz; yalnızca Finished maçlardan anlık hesaplanır.
    public class StandingsService
    {
        private readonly LaLigaContext _context;

        public StandingsService(LaLigaContext context)
        {
            _context = context;
        }

        public async Task<List<StandingDto>> GetStandingsAsync()
        {
            var teams = await _context.Teams
                .AsNoTracking()
                .Where(t => t.IsActive)
                .ToListAsync();

            var finished = await _context.Matches
                .AsNoTracking()
                .Where(m => m.Status == MatchStatus.Finished && m.HomeScore != null && m.AwayScore != null)
                .OrderBy(m => m.MatchDate)
                .ThenBy(m => m.Id)
                .Select(m => new
                {
                    m.HomeTeamId,
                    m.AwayTeamId,
                    HomeScore = m.HomeScore!.Value,
                    AwayScore = m.AwayScore!.Value
                })
                .ToListAsync();

            var standings = teams.Select(t =>
            {
                // Bu takımın maçları, eskiden yeniye: (attığı, yediği)
                var games = finished
                    .Where(m => m.HomeTeamId == t.Id || m.AwayTeamId == t.Id)
                    .Select(m => m.HomeTeamId == t.Id
                        ? new { Scored = m.HomeScore, Conceded = m.AwayScore }
                        : new { Scored = m.AwayScore, Conceded = m.HomeScore })
                    .ToList();

                int won = games.Count(g => g.Scored > g.Conceded);
                int drawn = games.Count(g => g.Scored == g.Conceded);
                int scored = games.Sum(g => g.Scored);
                int conceded = games.Sum(g => g.Conceded);

                return new StandingDto
                {
                    TeamId = t.Id,
                    TeamName = t.Name,
                    Code = t.Code,
                    LogoUrl = t.LogoUrl,
                    ColorA = t.ColorA,
                    ColorB = t.ColorB,
                    Played = games.Count,
                    Won = won,
                    Drawn = drawn,
                    Lost = games.Count - won - drawn,
                    GoalsFor = scored,
                    GoalsAgainst = conceded,
                    GoalDifference = scored - conceded,   // Averaj = Atılan - Yenilen
                    Points = won * 3 + drawn,             // Galibiyet 3, beraberlik 1, mağlubiyet 0
                    Form = games
                        .TakeLast(5)
                        .Select(g => g.Scored > g.Conceded ? "W" : g.Scored == g.Conceded ? "D" : "L")
                        .ToList()
                };
            })
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.GoalDifference)
            .ThenByDescending(s => s.GoalsFor)
            .ThenBy(s => s.TeamName)                      // tam eşitlikte sıra sabit kalsın
            .ToList();

            for (int i = 0; i < standings.Count; i++)
            {
                standings[i].Position = i + 1;
            }

            return standings;
        }
    }
}
