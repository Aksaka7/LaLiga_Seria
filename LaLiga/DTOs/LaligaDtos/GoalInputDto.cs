using System.ComponentModel.DataAnnotations;

namespace LaLiga.DTOs
{
    public class GoalInputDto
    {
        public int TeamId { get; set; }

        [Required, MaxLength(100)]
        public string PlayerName { get; set; } = string.Empty;

        [Range(1, 130)]
        public int Minute { get; set; }

        [MaxLength(100)]
        public string? AssistPlayerName { get; set; }

        // Örn. "kafa", "penaltı", "frikik"
        [MaxLength(100)]
        public string? Detail { get; set; }
    }
}
