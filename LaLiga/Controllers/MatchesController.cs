using System.Globalization;
using System.Text;
using LaLiga.Context;
using LaLiga.DTOs;
using LaLiga.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaLiga.Controllers
{
    [ApiController]
    [Route("api/matches")]
    public class MatchesController : ControllerBase
    {
        private readonly LaLigaContext _context;

        public MatchesController(LaLigaContext context)
        {
            _context = context;
        }

        // GET api/matches?week=3&status=Live&team=real  (admin listesi ve filtreler)
        [HttpGet]
        public async Task<ActionResult<List<MatchDto>>> GetMatches(
            [FromQuery] int? week, [FromQuery] MatchStatus? status, [FromQuery] string? team)
        {
            var query = _context.Matches
                .AsNoTracking()
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .AsQueryable();

            if (week.HasValue)
                query = query.Where(m => m.Week == week.Value);

            if (status.HasValue)
                query = query.Where(m => m.Status == status.Value);

            var matches = await query
                .OrderBy(m => m.Week)
                .ThenBy(m => m.MatchDate)
                .ThenBy(m => m.Id)
                .ToListAsync();

            // Takım adı araması aksan ve harf duyarsız
            if (!string.IsNullOrWhiteSpace(team))
            {
                var q = Normalize(team.Trim());
                matches = matches
                    .Where(m => Normalize(m.HomeTeam.Name).Contains(q) || Normalize(m.AwayTeam.Name).Contains(q))
                    .ToList();
            }

            return Ok(matches.Select(MapMatch<MatchDto>).ToList());
        }

        // GET api/matches/week/3  (haftalık fikstür)
        [HttpGet("week/{week:int}")]
        public async Task<ActionResult<List<MatchDto>>> GetMatchesByWeek(int week)
        {
            var matches = await _context.Matches
                .AsNoTracking()
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.Week == week)
                .OrderBy(m => m.MatchDate)
                .ThenBy(m => m.Id)
                .ToListAsync();

            if (matches.Count == 0)
                return NotFound(new { message = $"{week}. hafta için maç bulunamadı." });

            return Ok(matches.Select(MapMatch<MatchDto>).ToList());
        }

        // GET api/matches/5  (maç detayı: skor, goller, kartlar, değişiklikler, istatistik)
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MatchDetailDto>> GetMatch(int id)
        {
            var match = await _context.Matches
                .AsNoTracking()
                .AsSplitQuery()   // birden fazla koleksiyon birlikte çekilirken satır çoğalmasın
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.Statistic)
                .Include(m => m.Goals).ThenInclude(g => g.Team)
                .Include(m => m.Cards).ThenInclude(c => c.Team)
                .Include(m => m.Substitutions).ThenInclude(s => s.Team)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
                return NotFound(new { message = $"{id} numaralı maç bulunamadı." });

            var dto = MapMatch<MatchDetailDto>(match);

            // Goller dakikaya göre; her golde maçın anlık skoru hesaplanır
            var goals = match.Goals.OrderBy(g => g.Minute).ThenBy(g => g.Id).ToList();
            int homeGoals = 0, awayGoals = 0;
            foreach (var g in goals)
            {
                if (g.TeamId == match.HomeTeamId) homeGoals++; else awayGoals++;

                dto.Goals.Add(new GoalDto
                {
                    Id = g.Id,
                    TeamId = g.TeamId,
                    TeamName = g.Team.Name,
                    PlayerName = g.PlayerName,
                    Minute = g.Minute,
                    AssistPlayerName = g.AssistPlayerName,
                    Detail = g.Detail,
                    HomeScoreAfter = homeGoals,
                    AwayScoreAfter = awayGoals
                });
            }

            // Devre arası skoru: yalnızca golleri kayıtlı maçlarda
            if (goals.Count > 0)
            {
                dto.HalfTimeHomeScore = goals.Count(g => g.Minute <= 45 && g.TeamId == match.HomeTeamId);
                dto.HalfTimeAwayScore = goals.Count(g => g.Minute <= 45 && g.TeamId == match.AwayTeamId);
            }

            dto.Cards = match.Cards
                .OrderBy(c => c.Minute)
                .ThenBy(c => c.Id)
                .Select(c => new CardDto
                {
                    Id = c.Id,
                    TeamId = c.TeamId,
                    TeamName = c.Team.Name,
                    PlayerName = c.PlayerName,
                    Minute = c.Minute,
                    CardType = c.CardType,
                    Reason = c.Reason
                })
                .ToList();

            dto.Substitutions = match.Substitutions
                .OrderBy(s => s.Minute)
                .ThenBy(s => s.Id)
                .Select(s => new SubstitutionDto
                {
                    Id = s.Id,
                    TeamId = s.TeamId,
                    TeamName = s.Team.Name,
                    PlayerIn = s.PlayerIn,
                    PlayerOut = s.PlayerOut,
                    Minute = s.Minute
                })
                .ToList();

            if (match.Statistic != null)
            {
                var s = match.Statistic;
                dto.Statistic = new MatchStatisticDto
                {
                    HomePossession = s.HomePossession,
                    AwayPossession = 100 - s.HomePossession,
                    HomeShots = s.HomeShots,
                    AwayShots = s.AwayShots,
                    HomeShotsOnTarget = s.HomeShotsOnTarget,
                    AwayShotsOnTarget = s.AwayShotsOnTarget,
                    HomePasses = s.HomePasses,
                    AwayPasses = s.AwayPasses,
                    HomePassAccuracy = s.HomePassAccuracy,
                    AwayPassAccuracy = s.AwayPassAccuracy,
                    HomeCorners = s.HomeCorners,
                    AwayCorners = s.AwayCorners,
                    HomeFouls = s.HomeFouls,
                    AwayFouls = s.AwayFouls,
                    HomeOffsides = s.HomeOffsides,
                    AwayOffsides = s.AwayOffsides
                };
            }

            return Ok(dto);
        }

        // POST api/matches
        [HttpPost]
        public async Task<ActionResult<MatchDto>> CreateMatch(MatchInputDto input)
        {
            var error = await ValidateAsync(input, null);
            if (error != null)
                return BadRequest(new { message = error });

            var match = new Match();
            Apply(match, input);

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            var saved = await _context.Matches
                .AsNoTracking()
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstAsync(m => m.Id == match.Id);

            return CreatedAtAction(nameof(GetMatch), new { id = match.Id }, MapMatch<MatchDto>(saved));
        }

        // PUT api/matches/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMatch(int id, MatchInputDto input)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null)
                return NotFound(new { message = $"{id} numaralı maç bulunamadı." });

            var error = await ValidateAsync(input, match);
            if (error != null)
                return BadRequest(new { message = error });

            Apply(match, input);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/matches/5  (maçın golleri, kartları, değişiklikleri ve istatistiği veritabanında Cascade ile silinir)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMatch(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null)
                return NotFound(new { message = $"{id} numaralı maç bulunamadı." });

            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // İş kuralları. Hata yoksa null döner. existing: güncellenen maç (eklemede null).
        private async Task<string?> ValidateAsync(MatchInputDto input, Match? existing)
        {
            int excludeMatchId = existing?.Id ?? 0;

            // Kural 1: ev sahibi ve deplasman aynı olamaz
            if (input.HomeTeamId == input.AwayTeamId)
                return "Ev sahibi ve deplasman takımı aynı olamaz.";

            // Takımlar gerçekten var mı
            var teams = await _context.Teams
                .Where(t => t.Id == input.HomeTeamId || t.Id == input.AwayTeamId)
                .ToListAsync();

            if (teams.Count != 2)
                return "Ev sahibi veya deplasman takımı bulunamadı.";

            // Pasif takım yeni maça eklenemez (takımları değişmeyen eski maç düzenlenebilir)
            bool teamsChanged = existing == null
                || existing.HomeTeamId != input.HomeTeamId
                || existing.AwayTeamId != input.AwayTeamId;

            if (teamsChanged)
            {
                var inactive = teams.FirstOrDefault(t => !t.IsActive);
                if (inactive != null)
                    return $"{inactive.Name} takımı pasif, maça eklenemez.";
            }

            // Canlı ya da tamamlanmış maçta iki skor da olmalı
            bool needsScore = input.Status == MatchStatus.Live || input.Status == MatchStatus.Finished;
            if (needsScore && (input.HomeScore == null || input.AwayScore == null))
                return "Canlı veya tamamlanmış maçta iki takımın skoru da girilmelidir.";

            // Olayı olan maç "Başlamadı" ya da "Ertelendi" durumuna çekilemez
            bool noEventsAllowed = input.Status == MatchStatus.NotStarted || input.Status == MatchStatus.Postponed;
            if (noEventsAllowed && existing != null)
            {
                bool hasEvents =
                    await _context.MatchGoals.AnyAsync(g => g.MatchId == existing.Id) ||
                    await _context.MatchCards.AnyAsync(c => c.MatchId == existing.Id) ||
                    await _context.Substitutions.AnyAsync(s => s.MatchId == existing.Id) ||
                    await _context.MatchStatistics.AnyAsync(s => s.MatchId == existing.Id);

                if (hasEvents)
                    return "Bu maçın gol, kart, değişiklik ya da istatistik kayıtları var. Durumu 'Başlamadı' veya 'Ertelendi' yapmak için önce bu kayıtları silin.";
            }

            foreach (var team in teams)
            {
                bool alreadyPlaying = await _context.Matches.AnyAsync(m =>
                    m.Week == input.Week &&
                    m.Id != excludeMatchId &&
                    (m.HomeTeamId == team.Id || m.AwayTeamId == team.Id));

                if (alreadyPlaying)
                    return $"{team.Name} takımı {input.Week}. haftada zaten bir maçta yer alıyor.";
            }

            return null;
        }

        // Girdiyi maça uygular; duruma uymayan alanlar boşaltılır (skor yalnız Live/Finished, dakika yalnız Live, not yalnız Postponed)
        private static void Apply(Match match, MatchInputDto input)
        {
            bool scored = input.Status == MatchStatus.Live || input.Status == MatchStatus.Finished;

            match.HomeTeamId = input.HomeTeamId;
            match.AwayTeamId = input.AwayTeamId;
            match.Week = input.Week;
            match.MatchDate = input.MatchDate;
            match.Status = input.Status;
            match.Stadium = input.Stadium.Trim();

            match.HomeScore = scored ? input.HomeScore : null;
            match.AwayScore = scored ? input.AwayScore : null;
            match.Minute = input.Status == MatchStatus.Live ? input.Minute : null;
            match.Note = input.Status == MatchStatus.Postponed && !string.IsNullOrWhiteSpace(input.Note)
                ? input.Note.Trim()
                : null;

            match.Referee = string.IsNullOrWhiteSpace(input.Referee) ? null : input.Referee.Trim();
            match.Attendance = input.Attendance;
        }

        // Aksan ve harf duyarsız arama için
        private static string Normalize(string value)
        {
            var decomposed = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var chars = decomposed.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
            return new string(chars.ToArray());
        }

        private static T MapMatch<T>(Match m) where T : MatchDto, new()
        {
            return new T
            {
                Id = m.Id,
                Week = m.Week,
                MatchDate = m.MatchDate,
                HomeTeamId = m.HomeTeamId,
                HomeTeamName = m.HomeTeam.Name,
                HomeTeamCode = m.HomeTeam.Code,
                HomeTeamLogoUrl = m.HomeTeam.LogoUrl,
                HomeTeamColorA = m.HomeTeam.ColorA,
                HomeTeamColorB = m.HomeTeam.ColorB,
                AwayTeamId = m.AwayTeamId,
                AwayTeamName = m.AwayTeam.Name,
                AwayTeamCode = m.AwayTeam.Code,
                AwayTeamLogoUrl = m.AwayTeam.LogoUrl,
                AwayTeamColorA = m.AwayTeam.ColorA,
                AwayTeamColorB = m.AwayTeam.ColorB,
                HomeScore = m.HomeScore,
                AwayScore = m.AwayScore,
                Status = m.Status,
                Stadium = m.Stadium,
                City = m.HomeTeam.City,
                Minute = m.Minute,
                Note = m.Note,
                Referee = m.Referee,
                Attendance = m.Attendance
            };
        }
    }
}
