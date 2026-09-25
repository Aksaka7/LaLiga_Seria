using System.ComponentModel.DataAnnotations;
using LaLiga.Entities;

namespace LaLiga.DTOs
{
    public class MatchInputDto
    {
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }

        [Range(1, 38)]
        public int Week { get; set; }

        public DateTime MatchDate { get; set; }

        [Range(0, 99)]
        public int? HomeScore { get; set; }

        [Range(0, 99)]
        public int? AwayScore { get; set; }

        public MatchStatus Status { get; set; }

        [Required, MaxLength(100)]
        public string Stadium { get; set; } = string.Empty;

        // Canlı maçta oynanan dakika
        [Range(1, 130)]
        public int? Minute { get; set; }

        [MaxLength(200)]
        public string? Note { get; set; }

        [MaxLength(100)]
        public string? Referee { get; set; }

        [Range(0, 200000)]
        public int? Attendance { get; set; }
    }
}
