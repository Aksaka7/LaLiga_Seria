using LaLiga.Context;
using LaLiga.DTOs;
using LaLiga.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaLiga.Controllers
{
    [ApiController]
    [Route("api/teams")]
    public class TeamsController : ControllerBase
    {
        private const int MaxActiveTeams = 20;

        private readonly LaLigaContext _context;

        public TeamsController(LaLigaContext context)
        {
            _context = context;
        }

        // GET api/teams?activeOnly=true
        [HttpGet]
        public async Task<ActionResult<List<TeamDto>>> GetTeams([FromQuery] bool activeOnly = false)
        {
            var query = _context.Teams.AsNoTracking();

            if (activeOnly)
                query = query.Where(t => t.IsActive);

            var teams = await query.OrderBy(t => t.Name).ToListAsync();
            return Ok(teams.Select(ToDto).ToList());
        }

        // GET api/teams/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TeamDto>> GetTeam(int id)
        {
            var team = await _context.Teams.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (team == null)
                return NotFound(new { message = $"{id} numaralı takım bulunamadı." });

            return Ok(ToDto(team));
        }

        // POST api/teams
        [HttpPost]
        public async Task<ActionResult<TeamDto>> CreateTeam(TeamInputDto input)
        {
            var error = await ValidateAsync(input, 0, false);
            if (error != null)
                return BadRequest(new { message = error });

            var team = new Team();
            Apply(team, input);

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, ToDto(team));
        }

        // PUT api/teams/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTeam(int id, TeamInputDto input)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
                return NotFound(new { message = $"{id} numaralı takım bulunamadı." });

            var error = await ValidateAsync(input, id, team.IsActive);
            if (error != null)
                return BadRequest(new { message = error });

            Apply(team, input);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/teams/5  
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
                return NotFound(new { message = $"{id} numaralı takım bulunamadı." });

            bool hasMatches = await _context.Matches.AnyAsync(m => m.HomeTeamId == id || m.AwayTeamId == id);
            if (hasMatches)
                return Conflict(new { message = $"{team.Name} takımının maçları olduğu için silinemez. Önce maçlarını silin ya da takımı pasif yapın." });

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // İş kuralları. Hata yoksa null döner. excludeId: güncellemede takımın kendisini kontrolden hariç tutar (eklemede 0).
        private async Task<string?> ValidateAsync(TeamInputDto input, int excludeId, bool wasActive)
        {
            var name = input.Name.Trim();
            var code = input.Code.Trim().ToUpperInvariant();

            if (name.Length < 2)
                return "Takım adı en az 2 karakter olmalı.";

            // Ad tekil: harf ve aksan duyarsız karşılaştırma ("alaves" = "Alavés")
            bool nameTaken = await _context.Teams.AnyAsync(t =>
                t.Id != excludeId && EF.Functions.Collate(t.Name, "Latin1_General_CI_AI") == name);
            if (nameTaken)
                return "Bu isimde bir takım zaten kayıtlı.";

            bool codeTaken = await _context.Teams.AnyAsync(t => t.Id != excludeId && t.Code == code);
            if (codeTaken)
                return "Bu kısa ad başka bir takım tarafından kullanılıyor.";

            if (string.Equals(input.ColorA, input.ColorB, StringComparison.OrdinalIgnoreCase))
                return "Ana renk ve ikinci renk aynı olamaz.";

            // En çok 20 aktif takım: yeni aktif ya da pasiften aktife dönen takım için kontrol
            if (input.IsActive && !wasActive)
            {
                int activeCount = await _context.Teams.CountAsync(t => t.IsActive && t.Id != excludeId);
                if (activeCount >= MaxActiveTeams)
                    return $"La Liga'da en fazla {MaxActiveTeams} aktif takım olabilir. Önce başka bir takımı pasif yapın.";
            }

            return null;
        }

        private static void Apply(Team team, TeamInputDto input)
        {
            team.Name = input.Name.Trim();
            team.Code = input.Code.Trim().ToUpperInvariant();
            team.LogoUrl = string.IsNullOrWhiteSpace(input.LogoUrl) ? null : input.LogoUrl.Trim();
            team.City = input.City.Trim();
            team.Region = input.Region;
            team.Stadium = input.Stadium.Trim();
            team.Capacity = input.Capacity;
            team.FoundedYear = input.FoundedYear;
            team.ColorA = input.ColorA.ToUpperInvariant();
            team.ColorB = input.ColorB.ToUpperInvariant();
            team.IsActive = input.IsActive;
        }

        private static TeamDto ToDto(Team t)
        {
            return new TeamDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                LogoUrl = t.LogoUrl,
                City = t.City,
                Region = t.Region,
                Stadium = t.Stadium,
                Capacity = t.Capacity,
                FoundedYear = t.FoundedYear,
                ColorA = t.ColorA,
                ColorB = t.ColorB,
                IsActive = t.IsActive
            };
        }
    }
}
