using System.Globalization;

namespace LaLiga.WebUI.ViewModels
{
    // Puan Durumu sayfasının modeli: API'den gelen sıralama + sayfanın ihtiyaç duyduğu hesaplamalar
    public class StandingsPageViewModel
    {
        private static readonly CultureInfo Turkish = new("tr-TR");

        // API'den gelen sıralama (1. sıradan başlar)
        public List<StandingViewModel> Standings { get; set; } = new();

        public string LeagueName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public int CurrentWeek { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int TeamCount => Standings.Count;

        // "La Liga" -> "LL" (lig rozetindeki harfler)
        public string LeagueMark =>
            string.Concat(LeagueName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(word => word[0])).ToUpperInvariant();

        public string UpdatedAtText => UpdatedAt.ToString("d MMMM yyyy, HH:mm", Turkish);

        //  Bölgeler 

        public string ZoneOf(int position)
        {
            if (position <= 4) return "ucl";
            if (position <= 6) return "europe";
            if (position > TeamCount - 3) return "rel";
            return "mid";
        }

        public string ZoneClassOf(int position)
        {
            var zone = ZoneOf(position);
            return zone == "mid" ? string.Empty : $"zone-{zone}";
        }

        public string EdgeOf(int position)
        {
            if (position == 4) return "edge-ucl";
            if (position == 6) return "edge-europe";
            if (position == TeamCount - 3) return "edge-rel";
            return string.Empty;
        }

        //  Puan dağılımı çizgisi 

        public int LadderMin => Standings.Count == 0 ? 0 : Standings.Min(s => s.Points) - 1;
        public int LadderMax => Standings.Count == 0 ? 1 : Standings.Max(s => s.Points) + 1;

        // Puanın çizgideki yatay konumu (CSS için nokta ondalık ayırıcılı yüzde)
        public string LadderX(int points)
        {
            return Percent((double)(points - LadderMin) / (LadderMax - LadderMin) * 100);
        }

        // Çizgi üzerindeki puan işaretleri: aralık darsa her puan, genişse 5'er ya da 10'ar
        public IEnumerable<int> LadderTicks
        {
            get
            {
                int range = LadderMax - LadderMin;
                int step = range <= 12 ? 1 : range <= 30 ? 5 : 10;

                for (int points = LadderMin + 1; points < LadderMax; points++)
                {
                    if (points % step == 0)
                        yield return points;
                }
            }
        }

        // Aynı puandaki takımlar çizgide üst üste dizilir: 0, 1, 2...
        public int StackOf(StandingViewModel team)
        {
            return Standings.Count(s => s.Points == team.Points && s.Position < team.Position);
        }

        public int MaxStack => Standings.Count == 0 ? 0 : Standings.Max(StackOf);

        // Üst üste dizilen takımlar için çizgi yüksekliği 
        public int LadderTrackHeight => 116 + Math.Max(0, MaxStack - 1) * 34;

        //  Özet kartları 

        public StandingViewModel? Leader => Standings.FirstOrDefault();

        public int LeaderGap => Standings.Count < 2 ? 0 : Standings[0].Points - Standings[1].Points;

        public StandingViewModel? BestAttack =>
            Standings.OrderByDescending(s => s.GoalsFor).ThenBy(s => s.Position).FirstOrDefault();

        public StandingViewModel? BestDefence =>
            Standings.Where(s => s.Played > 0).OrderBy(s => s.GoalsAgainst).ThenBy(s => s.Position).FirstOrDefault();

        // Düşme hattının ilk takımı (20 takımda 18. sıra)
        public StandingViewModel? RelegationStart => Standings.Count > 3 ? Standings[TeamCount - 3] : null;

        public int SafetyGap => RelegationStart == null ? 0 : Standings[TeamCount - 4].Points - RelegationStart.Points;

        public string PerMatch(int goals, int played)
        {
            return played == 0 ? "0,00" : (goals / (double)played).ToString("0.00", Turkish);
        }

        //  Formda olanlar 

        public static int FormPoints(StandingViewModel team)
        {
            return team.Form.Sum(result => result == "W" ? 3 : result == "D" ? 1 : 0);
        }

        public List<StandingViewModel> FormLeaders =>
            Standings.Where(s => s.Form.Count > 0)
                     .OrderByDescending(FormPoints)
                     .ThenBy(s => s.Position)
                     .Take(3)
                     .ToList();


        public static string FormLetter(string result) => result switch { "W" => "G", "D" => "B", _ => "M" };

        public static string FormWord(string result) => result switch { "W" => "Galibiyet", "D" => "Beraberlik", _ => "Mağlubiyet" };

        public static string FormLabel(StandingViewModel team)
        {
            return $"Son {team.Form.Count} maç, eskiden yeniye: {string.Join(", ", team.Form.Select(FormWord))}";
        }

        //  Averaj 

        public static string GdText(int goalDifference)
        {
            return goalDifference > 0
                ? "+" + goalDifference.ToString(CultureInfo.InvariantCulture)
                : goalDifference.ToString(CultureInfo.InvariantCulture);
        }

        public static string GdClass(int goalDifference)
        {
            return goalDifference > 0 ? "is-pos" : goalDifference < 0 ? "is-neg" : "is-zero";
        }

        // ---------- Yardımcılar ----------

        public CrestViewModel CrestOf(StandingViewModel team, string sizeClass = "")
        {
            return new CrestViewModel
            {
                Code = team.Code,
                ColorA = team.ColorA,
                ColorB = team.ColorB,
                LogoUrl = team.LogoUrl,
                SizeClass = sizeClass
            };
        }

        // CSS için yüzde: her zaman nokta ondalık ayırıcı (Türkçe ayarda virgül CSS'i bozar)
        public static string Percent(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture) + "%";
        }

        public static string Percent(int part, int total)
        {
            return total == 0 ? "0%" : Percent(part * 100.0 / total);
        }
    }
}
