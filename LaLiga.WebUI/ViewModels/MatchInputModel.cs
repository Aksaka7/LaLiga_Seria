namespace LaLiga.WebUI.ViewModels
{
    // API'ye maç eklerken ya da güncellerken gönderilen gövde (API'deki MatchInputDto ile aynı alanlar görünecek
    public class MatchInputModel
    {
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public int Week { get; set; }
        public DateTime MatchDate { get; set; }
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        public MatchStatus Status { get; set; }
        public string Stadium { get; set; } = string.Empty;
        public int? Minute { get; set; }
        public string? Note { get; set; }
        public string? Referee { get; set; }
        public int? Attendance { get; set; }
    }
}
