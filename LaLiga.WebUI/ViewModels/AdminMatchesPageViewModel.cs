namespace LaLiga.WebUI.ViewModels
{
    public class AdminMatchesPageViewModel
    {
        public List<MatchViewModel> Matches { get; set; } = new();

        public int TotalCount { get; set; }

        public int TotalWeeks { get; set; }

        // Ekle/Düzenle formundaki Ev Sahibi / Deplasman açılır listeleri için  
        public List<TeamViewModel> Teams { get; set; } = new();

        // Filtre formunun geri doldurulması için
        public string? FilterTeam { get; set; }
        public int? FilterWeek { get; set; }
        public MatchStatus? FilterStatus { get; set; }

        public bool Empty => Matches.Count == 0;

        public string FooterText => $"{TotalCount} kayıttan {Matches.Count} tanesi gösteriliyor";

        public IEnumerable<int> WeekOptions => Enumerable.Range(1, TotalWeeks);

        public static readonly (MatchStatus Value, string Label)[] StatusOptions =
        {
            (MatchStatus.NotStarted, "Başlamadı"),
            (MatchStatus.Live, "Canlı"),
            (MatchStatus.Finished, "Tamamlandı"),
            (MatchStatus.Postponed, "Ertelendi")
        };

        public string StatusLabel(MatchStatus status) => status switch
        {
            MatchStatus.Live => "Canlı",
            MatchStatus.Finished => "Tamamlandı",
            MatchStatus.Postponed => "Ertelendi",
            _ => "Başlamadı"
        };

        public bool HasScore(MatchViewModel match) => match.Status == MatchStatus.Live || match.Status == MatchStatus.Finished;

        // Tablo görünümü (Türkçe): "27.09.2026"
        public string DateText(MatchViewModel match) => match.MatchDate.ToString("dd.MM.yyyy");

        public string TimeText(MatchViewModel match) => match.MatchDate.ToString("HH:mm");

        public string IsoDate(MatchViewModel match) => match.MatchDate.ToString("yyyy-MM-dd");

        public string IsoTime(MatchViewModel match) => match.MatchDate.ToString("HH:mm");

        //  Puan Durumu/Fikstür'de kullanılanlar
        public CrestViewModel HomeCrest(MatchViewModel match, string sizeClass = "team-crest--sm") => new()
        {
            Code = match.HomeTeamCode,
            ColorA = match.HomeTeamColorA,
            ColorB = match.HomeTeamColorB,
            LogoUrl = match.HomeTeamLogoUrl,
            SizeClass = sizeClass
        };

        public CrestViewModel AwayCrest(MatchViewModel match, string sizeClass = "team-crest--sm") => new()
        {
            Code = match.AwayTeamCode,
            ColorA = match.AwayTeamColorA,
            ColorB = match.AwayTeamColorB,
            LogoUrl = match.AwayTeamLogoUrl,
            SizeClass = sizeClass
        };
    }
}
