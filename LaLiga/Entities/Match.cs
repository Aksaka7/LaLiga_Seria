using System.ComponentModel.DataAnnotations;

namespace LaLiga.Entities
{
    public class Match
    {
        public int Id { get; set; }

        public int HomeTeamId { get; set; }
        public Team HomeTeam { get; set; } = null!;

        public int AwayTeamId { get; set; }
        public Team AwayTeam { get; set; } = null!;

        public int Week { get; set; }

        public DateTime MatchDate { get; set; }

        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }

        public MatchStatus Status { get; set; } = MatchStatus.NotStarted;

        [Required, MaxLength(100)]
        public string Stadium { get; set; } = string.Empty;

        // Canlı maçta oynanan dakika
        public int? Minute { get; set; }

        // Erteleme sebebi vb.
        [MaxLength(200)]
        public string? Note { get; set; }

        [MaxLength(100)]
        public string? Referee { get; set; }

        public int? Attendance { get; set; }

        public MatchStatistic? Statistic { get; set; }

        public ICollection<MatchGoal> Goals { get; set; } = new List<MatchGoal>();
        public ICollection<MatchCard> Cards { get; set; } = new List<MatchCard>();
        public ICollection<Substitution> Substitutions { get; set; } = new List<Substitution>();
    }
}
