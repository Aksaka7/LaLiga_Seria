namespace LaLiga.DTOs
{
    public class MatchDetailDto : MatchDto
    {
        // Devre arası skoru: yalnızca golleri kayıtlı maçlarda dolu (45. dakika ve öncesi)
        public int? HalfTimeHomeScore { get; set; }
        public int? HalfTimeAwayScore { get; set; }

        public MatchStatisticDto? Statistic { get; set; }

        public List<GoalDto> Goals { get; set; } = new();
        public List<CardDto> Cards { get; set; } = new();
        public List<SubstitutionDto> Substitutions { get; set; } = new();
    }
}
