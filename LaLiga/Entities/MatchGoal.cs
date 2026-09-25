using System.ComponentModel.DataAnnotations;

namespace LaLiga.Entities
{
    public class MatchGoal
    {
        public int Id { get; set; }

        public int MatchId { get; set; }
        public Match Match { get; set; } = null!;

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        [Required, MaxLength(100)]
        public string PlayerName { get; set; } = string.Empty;

        public int Minute { get; set; }

        [MaxLength(100)]
        public string? AssistPlayerName { get; set; }

        // Örn. "kafa", "penaltı"
        [MaxLength(100)]
        public string? Detail { get; set; }
    }
}
