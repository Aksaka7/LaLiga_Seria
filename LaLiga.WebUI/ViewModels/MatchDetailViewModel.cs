namespace LaLiga.WebUI.ViewModels
{
    public class MatchDetailViewModel : MatchViewModel
    {
        // Devre arası skoru: yalnızca golleri kayıtlı maçlarda dolu
        public int? HalfTimeHomeScore { get; set; }
        public int? HalfTimeAwayScore { get; set; }

        public MatchStatisticViewModel? Statistic { get; set; }

        public List<GoalViewModel> Goals { get; set; } = new();
        public List<CardViewModel> Cards { get; set; } = new();
        public List<SubstitutionViewModel> Substitutions { get; set; } = new();
    }
}
