using System.Globalization;

namespace LaLiga.WebUI.ViewModels
{
    // Haftalık Fikstür sayfasının modeli: API'den gelen haftalar + seçili haftanın maçları ve sayfanın ihtiyaç duyduğu hesaplamalar
    public class FixturesPageViewModel
    {
        private static readonly CultureInfo Turkish = new("tr-TR");

        // API'den gelen tüm haftalar 
        public List<WeekViewModel> Weeks { get; set; } = new();

        // Yalnızca seçili haftanın maçları
        public List<MatchViewModel> Matches { get; set; } = new();

        public string LeagueName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public int TeamCount { get; set; }
        public int TotalWeeks { get; set; }
        public int CurrentWeek { get; set; }
        public int SelectedWeek { get; set; }

        // "La Liga"  rozeti
        public string LeagueMark =>
            string.Concat(LeagueName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(word => word[0])).ToUpperInvariant();

        //  Hafta seçici 

        public WeekViewModel? SelectedWeekInfo => Weeks.FirstOrDefault(w => w.Week == SelectedWeek);

        // Seçicide hep 3 hafta görünür
        public List<WeekViewModel> VisibleWeeks
        {
            get
            {
                int index = Weeks.FindIndex(w => w.Week == SelectedWeek);

                if (index < 0)
                    return new List<WeekViewModel>();

                int start = Math.Clamp(index - 1, 0, Math.Max(0, Weeks.Count - 3));
                return Weeks.Skip(start).Take(3).ToList();
            }
        }

        public int? PrevWeek => Weeks.LastOrDefault(w => w.Week < SelectedWeek)?.Week;

        public int? NextWeek => Weeks.FirstOrDefault(w => w.Week > SelectedWeek)?.Week;

        // "31 Ekim – 2 Kasım 2026"
        public string WeekRange(WeekViewModel week)
        {
            return $"{week.StartDate.ToString("d MMMM", Turkish)} – {week.EndDate.ToString("d MMMM yyyy", Turkish)}";
        }

        //  Maçlar 

        public List<MatchViewModel> OrderedMatches =>
            Matches.OrderBy(m => m.MatchDate).ThenBy(m => m.Id).ToList();

        // Gün gün gruplar (tarih sırasıyla)
        public IEnumerable<IGrouping<DateTime, MatchViewModel>> Days =>
            OrderedMatches.GroupBy(m => m.MatchDate.Date);

        // Skor yalnızca canlı ve biten maçlarda gösterilir
        public bool ShowsScore(MatchViewModel match)
        {
            return match.HasScore && match.HomeScore.HasValue && match.AwayScore.HasValue;
        }

        // Biten ve canlı maçların detay sayfası vardır
        public bool IsLinked(MatchViewModel match)
        {
            return match.Status == MatchStatus.Finished || match.Status == MatchStatus.Live;
        }

        public string DetailUrl(MatchViewModel match) => $"/Match/Detail/{match.Id}";

        public string LinkText(MatchViewModel match) => match.Status == MatchStatus.Live ? "Canlı takip" : "Maç detayı";

        // Skoru olan biten maçta kazanan takımın adı kalın, kaybeden soluk görünür
        private static int? GoalDifference(MatchViewModel match)
        {
            return match.Status == MatchStatus.Finished && match.HomeScore.HasValue && match.AwayScore.HasValue
                ? match.HomeScore - match.AwayScore
                : null;
        }

        public string HomeResultClass(MatchViewModel match) =>
            GoalDifference(match) switch { > 0 => "is-winner", < 0 => "is-loser", _ => string.Empty };

        public string AwayResultClass(MatchViewModel match) =>
            GoalDifference(match) switch { > 0 => "is-loser", < 0 => "is-winner", _ => string.Empty };

        //  Durum yazıları 

        public string StatusWord(MatchStatus status) => status switch
        {
            MatchStatus.Live => "Canlı",
            MatchStatus.Finished => "Maç bitti",
            MatchStatus.Postponed => "Ertelendi",
            _ => "Başlamadı"
        };

        // Canlı maçta dakika da yazılır: "Canlı 67'"
        public string StatusLabel(MatchViewModel match)
        {
            return match.Status == MatchStatus.Live && match.Minute.HasValue
                ? $"Canlı {MinuteText(match.Minute.Value)}'"
                : StatusWord(match.Status);
        }

        // Uzatma dakikaları: 93 -> "90+3"
        public string MinuteText(int minute)
        {
            return minute > 90 ? $"90+{minute - 90}" : minute.ToString(CultureInfo.InvariantCulture);
        }

        //  Tarih ve saat yazıları 

        public string DayTitle(DateTime date) => date.ToString("d MMMM dddd", Turkish);

        public string DayMonthText(DateTime date) => date.ToString("d MMMM", Turkish);

        public string WeekdayText(DateTime date) => date.ToString("dddd", Turkish);

        public string TimeText(DateTime date) => date.ToString("HH:mm", Turkish);

        public string IsoTime(DateTime date) => date.ToString("yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture);

        //  Haftalık özet 

        // Skor yazılabilen (canlı + biten) maçlar
        private IEnumerable<MatchViewModel> Scored => Matches.Where(ShowsScore);

        // Sonucu belli olan (biten) maçlar
        private IEnumerable<MatchViewModel> Decided => Scored.Where(m => m.Status == MatchStatus.Finished);

        public bool HasWeek => SelectedWeekInfo != null;

        public int Total => Matches.Count;

        public int FinishedCount => Matches.Count(m => m.Status == MatchStatus.Finished);

        public int LiveCount => Matches.Count(m => m.Status == MatchStatus.Live);

        // Gol sayısına canlı maçlar dahildir
        public int Goals => Scored.Sum(m => m.HomeScore!.Value + m.AwayScore!.Value);

        public string AverageGoals
        {
            get
            {
                int played = Scored.Count();
                return played == 0 ? "0,00" : (Goals / (double)played).ToString("0.00", Turkish);
            }
        }

        // Sonuç dağılımı yalnızca biten maçlardan hesaplanır
        public int Draws => Decided.Count(m => m.HomeScore == m.AwayScore);

        public int HomeWins => Decided.Count(m => m.HomeScore > m.AwayScore);

        public int AwayWins => Decided.Count(m => m.HomeScore < m.AwayScore);

        public string DoneText => FinishedCount == Total ? "Hafta tamamlandı" : $"{FinishedCount} / {Total} maç tamamlandı";

        //  Öne çıkan maç 

        // API'nin seçtiği maç; bulunamazsa haftanın ilk maçı
        public MatchViewModel? Featured =>
            Matches.FirstOrDefault(m => m.Id == SelectedWeekInfo?.FeaturedMatchId) ?? OrderedMatches.FirstOrDefault();

        //  Armalar 

        public CrestViewModel HomeCrestOf(MatchViewModel match, string sizeClass = "")
        {
            return new CrestViewModel
            {
                Code = match.HomeTeamCode,
                ColorA = match.HomeTeamColorA,
                ColorB = match.HomeTeamColorB,
                LogoUrl = match.HomeTeamLogoUrl,
                SizeClass = sizeClass
            };
        }

        public CrestViewModel AwayCrestOf(MatchViewModel match, string sizeClass = "")
        {
            return new CrestViewModel
            {
                Code = match.AwayTeamCode,
                ColorA = match.AwayTeamColorA,
                ColorB = match.AwayTeamColorB,
                LogoUrl = match.AwayTeamLogoUrl,
                SizeClass = sizeClass
            };
        }
    }
}
