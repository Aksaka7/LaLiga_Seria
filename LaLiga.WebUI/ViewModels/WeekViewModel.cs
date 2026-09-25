namespace LaLiga.WebUI.ViewModels
{
    public class WeekViewModel
    {
        public int Week { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MatchCount { get; set; }
        public int FinishedCount { get; set; }
        public int LiveCount { get; set; }
        public bool IsCurrent { get; set; }
        public int? FeaturedMatchId { get; set; }
    }
}
