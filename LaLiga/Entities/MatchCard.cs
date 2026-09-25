using System.ComponentModel.DataAnnotations;

namespace LaLiga.Entities
{
    public class MatchCard
    {
        public int Id { get; set; }

        public int MatchId { get; set; }
        public Match Match { get; set; } = null!;

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        [Required, MaxLength(100)]
        public string PlayerName { get; set; } = string.Empty;

        public int Minute { get; set; }

        public CardType CardType { get; set; }

        // Örn. "faul", "itiraz", "oyunu geciktirme"
        [MaxLength(100)]
        public string? Reason { get; set; }
    }
}
