using System.ComponentModel.DataAnnotations;

namespace LaLiga.Entities
{
    public class Substitution
    {
        public int Id { get; set; }

        public int MatchId { get; set; }
        public Match Match { get; set; } = null!;

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        [Required, MaxLength(100)]
        public string PlayerIn { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string PlayerOut { get; set; } = string.Empty;

        public int Minute { get; set; }
    }
}
