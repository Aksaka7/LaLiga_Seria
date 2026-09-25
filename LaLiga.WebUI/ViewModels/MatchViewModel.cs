namespace LaLiga.WebUI.ViewModels
{
    public class MatchViewModel
    {
        public int Id { get; set; }
        public int Week { get; set; }
        public DateTime MatchDate { get; set; }

        public int HomeTeamId { get; set; }
        public string HomeTeamName { get; set; } = string.Empty;
        public string HomeTeamCode { get; set; } = string.Empty;
        public string? HomeTeamLogoUrl { get; set; }
        public string HomeTeamColorA { get; set; } = string.Empty;
        public string HomeTeamColorB { get; set; } = string.Empty;

        public int AwayTeamId { get; set; }
        public string AwayTeamName { get; set; } = string.Empty;
        public string AwayTeamCode { get; set; } = string.Empty;
        public string? AwayTeamLogoUrl { get; set; }
        public string AwayTeamColorA { get; set; } = string.Empty;
        public string AwayTeamColorB { get; set; } = string.Empty;

        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }

        public MatchStatus Status { get; set; }
        public string Stadium { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        public int? Minute { get; set; }
        public string? Note { get; set; }
        public string? Referee { get; set; }
        public int? Attendance { get; set; }

        // Skor yalnızca canlı ve biten maçlarda gösterilir
        public bool HasScore => Status == MatchStatus.Live || Status == MatchStatus.Finished;
    }
}
