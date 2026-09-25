using LaLiga.Context;
using LaLiga.DTOs;
using LaLiga.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaLiga.Controllers
{
    // Bir maçın gol, kart, oyuncu değişikliği ve istatistik kayıtları.
    // Tüm kayıtlar geçerli bir maça bağlı olmak zorundadır (maç yoksa 404).
    [ApiController]
    [Route("api/matches/{matchId:int}")]
    public class MatchEventsController : ControllerBase
    {
        private readonly LaLigaContext _context;

        public MatchEventsController(LaLigaContext context)
        {
            _context = context;
        }

        // ---------- GOLLER ----------

        // POST api/matches/5/goals
        [HttpPost("goals")]
        public async Task<IActionResult> AddGoal(int matchId, GoalInputDto input)
        {
            var match = await FindMatchAsync(matchId);
            if (match == null)
                return MatchNotFound(matchId);

            var error = Validate(match, input.TeamId);
            if (error != null)
                return BadRequest(new { message = error });

            var goal = new MatchGoal
            {
                MatchId = matchId,
                TeamId = input.TeamId,
                PlayerName = input.PlayerName.Trim(),
                Minute = input.Minute,
                AssistPlayerName = Clean(input.AssistPlayerName),
                Detail = Clean(input.Detail)
            };

            _context.MatchGoals.Add(goal);
            await _context.SaveChangesAsync();
            return Created($"/api/matches/{matchId}", new { id = goal.Id });
        }

        // DELETE api/matches/5/goals/12
        [HttpDelete("goals/{goalId:int}")]
        public async Task<IActionResult> DeleteGoal(int matchId, int goalId)
        {
            var goal = await _context.MatchGoals.FirstOrDefaultAsync(g => g.Id == goalId && g.MatchId == matchId);
            if (goal == null)
                return NotFound(new { message = $"{matchId} numaralı maçta {goalId} numaralı gol bulunamadı." });

            _context.MatchGoals.Remove(goal);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ---------- KARTLAR ----------

        // POST api/matches/5/cards
        [HttpPost("cards")]
        public async Task<IActionResult> AddCard(int matchId, CardInputDto input)
        {
            var match = await FindMatchAsync(matchId);
            if (match == null)
                return MatchNotFound(matchId);

            var error = Validate(match, input.TeamId);
            if (error != null)
                return BadRequest(new { message = error });

            var card = new MatchCard
            {
                MatchId = matchId,
                TeamId = input.TeamId,
                PlayerName = input.PlayerName.Trim(),
                Minute = input.Minute,
                CardType = input.CardType,
                Reason = Clean(input.Reason)
            };

            _context.MatchCards.Add(card);
            await _context.SaveChangesAsync();
            return Created($"/api/matches/{matchId}", new { id = card.Id });
        }

        // DELETE api/matches/5/cards/12
        [HttpDelete("cards/{cardId:int}")]
        public async Task<IActionResult> DeleteCard(int matchId, int cardId)
        {
            var card = await _context.MatchCards.FirstOrDefaultAsync(c => c.Id == cardId && c.MatchId == matchId);
            if (card == null)
                return NotFound(new { message = $"{matchId} numaralı maçta {cardId} numaralı kart bulunamadı." });

            _context.MatchCards.Remove(card);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ---------- OYUNCU DEĞİŞİKLİKLERİ ----------

        // POST api/matches/5/substitutions
        [HttpPost("substitutions")]
        public async Task<IActionResult> AddSubstitution(int matchId, SubstitutionInputDto input)
        {
            var match = await FindMatchAsync(matchId);
            if (match == null)
                return MatchNotFound(matchId);

            var error = Validate(match, input.TeamId);
            if (error != null)
                return BadRequest(new { message = error });

            if (string.Equals(input.PlayerIn.Trim(), input.PlayerOut.Trim(), StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Giren ve çıkan oyuncu aynı olamaz." });

            var substitution = new Substitution
            {
                MatchId = matchId,
                TeamId = input.TeamId,
                PlayerIn = input.PlayerIn.Trim(),
                PlayerOut = input.PlayerOut.Trim(),
                Minute = input.Minute
            };

            _context.Substitutions.Add(substitution);
            await _context.SaveChangesAsync();
            return Created($"/api/matches/{matchId}", new { id = substitution.Id });
        }

        // DELETE api/matches/5/substitutions/12
        [HttpDelete("substitutions/{substitutionId:int}")]
        public async Task<IActionResult> DeleteSubstitution(int matchId, int substitutionId)
        {
            var substitution = await _context.Substitutions
                .FirstOrDefaultAsync(s => s.Id == substitutionId && s.MatchId == matchId);
            if (substitution == null)
                return NotFound(new { message = $"{matchId} numaralı maçta {substitutionId} numaralı oyuncu değişikliği bulunamadı." });

            _context.Substitutions.Remove(substitution);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ---------- İSTATİSTİK (maça 1:1) ----------

        // PUT api/matches/5/statistic  (yoksa oluşturur 201, varsa günceller 204)
        [HttpPut("statistic")]
        public async Task<IActionResult> SaveStatistic(int matchId, MatchStatisticInputDto input)
        {
            var match = await FindMatchAsync(matchId);
            if (match == null)
                return MatchNotFound(matchId);

            var error = Validate(match, null);
            if (error != null)
                return BadRequest(new { message = error });

            if (input.HomeShotsOnTarget > input.HomeShots || input.AwayShotsOnTarget > input.AwayShots)
                return BadRequest(new { message = "İsabetli şut sayısı toplam şut sayısından fazla olamaz." });

            var statistic = await _context.MatchStatistics.FirstOrDefaultAsync(s => s.MatchId == matchId);
            bool created = statistic == null;

            if (statistic == null)
            {
                statistic = new MatchStatistic { MatchId = matchId };
                _context.MatchStatistics.Add(statistic);
            }

            statistic.HomePossession = input.HomePossession;
            statistic.HomeShots = input.HomeShots;
            statistic.AwayShots = input.AwayShots;
            statistic.HomeShotsOnTarget = input.HomeShotsOnTarget;
            statistic.AwayShotsOnTarget = input.AwayShotsOnTarget;
            statistic.HomePasses = input.HomePasses;
            statistic.AwayPasses = input.AwayPasses;
            statistic.HomePassAccuracy = input.HomePassAccuracy;
            statistic.AwayPassAccuracy = input.AwayPassAccuracy;
            statistic.HomeCorners = input.HomeCorners;
            statistic.AwayCorners = input.AwayCorners;
            statistic.HomeFouls = input.HomeFouls;
            statistic.AwayFouls = input.AwayFouls;
            statistic.HomeOffsides = input.HomeOffsides;
            statistic.AwayOffsides = input.AwayOffsides;

            await _context.SaveChangesAsync();

            return created
                ? Created($"/api/matches/{matchId}", new { id = statistic.Id })
                : NoContent();
        }

        // DELETE api/matches/5/statistic
        [HttpDelete("statistic")]
        public async Task<IActionResult> DeleteStatistic(int matchId)
        {
            var statistic = await _context.MatchStatistics.FirstOrDefaultAsync(s => s.MatchId == matchId);
            if (statistic == null)
                return NotFound(new { message = $"{matchId} numaralı maçın istatistiği bulunamadı." });

            _context.MatchStatistics.Remove(statistic);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ---------- Yardımcılar ----------

        private async Task<Match?> FindMatchAsync(int matchId)
        {
            return await _context.Matches.AsNoTracking().FirstOrDefaultAsync(m => m.Id == matchId);
        }

        private NotFoundObjectResult MatchNotFound(int matchId)
        {
            return NotFound(new { message = $"{matchId} numaralı maç bulunamadı." });
        }

        // Hata yoksa null. teamId verilirse takımın bu maçın takımlarından biri olması gerekir.
        private static string? Validate(Match match, int? teamId)
        {
            if (match.Status == MatchStatus.NotStarted || match.Status == MatchStatus.Postponed)
                return "Başlamamış ya da ertelenmiş maça gol, kart, değişiklik veya istatistik eklenemez.";

            if (teamId.HasValue && teamId != match.HomeTeamId && teamId != match.AwayTeamId)
                return "Takım, bu maçın takımlarından biri olmalı.";

            return null;
        }

        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
