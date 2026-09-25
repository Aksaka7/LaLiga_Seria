namespace LaLiga.DTOs
{
    public class WeekDto
    {
        public int Week { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MatchCount { get; set; }
        public int FinishedCount { get; set; }
        public int LiveCount { get; set; }
        public bool IsCurrent { get; set; }

        // Haftanın öne çıkan maçı: puan durumunda sıraları toplamı en düşük olan çift
        public int? FeaturedMatchId { get; set; }
    }
}
