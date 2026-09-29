using System.Globalization;

namespace LaLiga.WebUI.ViewModels
{
    // Zaman çizelgesindeki ve maç akışı şeridindeki tek bir olay (gol, kart ya da oyuncu değişikliği)
    public class MatchEventViewModel
    {
        public string Kind { get; set; } = string.Empty;      // "goal", "card" ya da "sub" (olay filtresi bunu kullanır)
        public string Variant { get; set; } = string.Empty;   // "goal", "yellow", "red" ya da "sub" (CSS sınıfı)
        public bool IsHome { get; set; }
        public int Minute { get; set; }
        public string DomId { get; set; } = string.Empty;     // "ev-goal-3": şerit işareti ile zaman çizelgesi bu adla eşleşir
        public string Player { get; set; } = string.Empty;    // gol/kart: oyuncu, değişiklik: giren oyuncu
        public string Meta { get; set; } = string.Empty;      // "Gol, asist: ..." / "Sarı kart, faul" / çıkan oyuncu
        public string? Score { get; set; }                    // yalnızca golde: "1-0"
        public string AriaLabel { get; set; } = string.Empty; // şerit işaretinin okuyucu metni
        public string Title { get; set; } = string.Empty;     // şerit işaretinin ipucu metni
        public bool CloseToPrevious { get; set; }             // aynı tarafta bir öncekine çok yakınsa şeritte biraz kaydırılır

        public string Side => IsHome ? "home" : "away";
    }

    // Sağ sütundaki "İlk yarı / İkinci yarı / Sonuç" kartlarından biri
    public class PeriodCardViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Score { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public bool IsFinal { get; set; }
    }

    // İstatistik listesindeki tek satır: iki takımın değeri + çubuk genişlikleri
    public class StatRowViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string HomeText { get; set; } = string.Empty;
        public string AwayText { get; set; } = string.Empty;
        public string HomeWidth { get; set; } = "0%";
        public string AwayWidth { get; set; } = "0%";
        public bool HomeLeads { get; set; }
        public bool AwayLeads { get; set; }
        public bool Muted { get; set; }
    }

    // Maç Detayı sayfasının modeli: API'den gelen maç + sayfanın ihtiyaç duyduğu hesaplamalar
    public class MatchDetailPageViewModel
    {
        private static readonly CultureInfo Turkish = new("tr-TR");

        public MatchDetailViewModel Match { get; set; } = new();

        public string LeagueName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;

        public string PageTitle =>
            ShowsScore
                ? $"{Match.HomeTeamName} {Match.HomeScore}-{Match.AwayScore} {Match.AwayTeamName} | Maç Detayı"
                : $"{Match.HomeTeamName} - {Match.AwayTeamName} | Maç Detayı";

        //  Durum ve skor 

        public bool IsFinished => Match.Status == MatchStatus.Finished;

        // Skor yalnızca canlı ve biten maçlarda gösterilir
        public bool ShowsScore => Match.HasScore && Match.HomeScore.HasValue && Match.AwayScore.HasValue;

        public string StatusText => Match.Status switch
        {
            MatchStatus.Live => Match.Minute.HasValue ? $"Canlı {MinuteText(Match.Minute.Value)}'" : "Canlı",
            MatchStatus.Finished => "Maç bitti",
            MatchStatus.Postponed => "Ertelendi",
            _ => "Başlamadı"
        };

        // Uzatma dakikaları: 93 -> "90+3"
        public string MinuteText(int minute) => minute > 90 ? $"90+{minute - 90}" : minute.ToString(CultureInfo.InvariantCulture);

        // Dakika etiketi: "14'" ya da "90+3'"
        public string MinuteLabel(int minute) => MinuteText(minute) + "'";


        // Biten ve berabere olmayan maçta kaybeden takımın adı ve skoru soluk görünür
        private int? GoalDifference =>
            IsFinished && Match.HomeScore.HasValue && Match.AwayScore.HasValue ? Match.HomeScore - Match.AwayScore : null;

        public bool HomeLost => GoalDifference < 0;

        public bool AwayLost => GoalDifference > 0;

        //  Devre arası 

        public bool HasHalfTime => Match.HalfTimeHomeScore.HasValue && Match.HalfTimeAwayScore.HasValue;

        // Devre arası: maç bitmişse ya da 45. dakika geçilmişse yaşanmıştır
        public bool HalfTimePassed => IsFinished || Match.Minute > 45;

        public bool ShowsHalfTime => HasHalfTime && HalfTimePassed;

        public string HalfTimeText => $"{Match.HalfTimeHomeScore}-{Match.HalfTimeAwayScore}";

        //  İlk yarı / ikinci yarı / sonuç kartları 

        public List<PeriodCardViewModel> PeriodCards
        {
            get
            {
                var cards = new List<PeriodCardViewModel>();

                if (!ShowsScore)
                    return cards;

                int home = Match.HomeScore!.Value;
                int away = Match.AwayScore!.Value;

                if (HasHalfTime)
                {
                    int firstHome = Match.HalfTimeHomeScore!.Value;
                    int firstAway = Match.HalfTimeAwayScore!.Value;

                    cards.Add(new PeriodCardViewModel { Label = "İlk yarı", Score = $"{firstHome}-{firstAway}", Note = LeaderNote(firstHome, firstAway) });

                    cards.Add(HalfTimePassed
                        ? new PeriodCardViewModel { Label = "İkinci yarı", Score = $"{home - firstHome}-{away - firstAway}", Note = LeaderNote(home - firstHome, away - firstAway) }
                        : new PeriodCardViewModel { Label = "İkinci yarı", Score = "-", Note = "Başlamadı" });
                }
                else
                {
                    cards.Add(new PeriodCardViewModel { Label = "İlk yarı", Score = "-", Note = "Veri yok" });
                    cards.Add(new PeriodCardViewModel { Label = "İkinci yarı", Score = "-", Note = "Veri yok" });
                }

                cards.Add(new PeriodCardViewModel
                {
                    Label = IsFinished ? "Sonuç" : "Anlık skor",
                    Score = $"{home}-{away}",
                    Note = IsFinished ? WinnerNote(home, away) : "Maç sürüyor",
                    IsFinal = true
                });

                return cards;
            }
        }

        private string LeaderNote(int home, int away)
        {
            if (home > away) return $"{Match.HomeTeamName} önde";
            if (home < away) return $"{Match.AwayTeamName} önde";
            return "Berabere";
        }

        private string WinnerNote(int home, int away)
        {
            if (home > away) return $"{Match.HomeTeamName} kazandı";
            if (home < away) return $"{Match.AwayTeamName} kazandı";
            return "Berabere";
        }

        //  Olaylar 

        private List<MatchEventViewModel>? _events;

        // Gol, kart ve değişiklikler dakika sırasıyla tek listede
        public List<MatchEventViewModel> Events => _events ??= BuildEvents();

        private List<MatchEventViewModel> BuildEvents()
        {
            var events = new List<MatchEventViewModel>();

            foreach (var goal in Match.Goals)
            {
                var meta = "Gol";

                if (!string.IsNullOrWhiteSpace(goal.Detail))
                    meta += $", {goal.Detail}";

                if (!string.IsNullOrWhiteSpace(goal.AssistPlayerName))
                    meta += $", asist: {goal.AssistPlayerName}";

                events.Add(new MatchEventViewModel
                {
                    Kind = "goal",
                    Variant = "goal",
                    IsHome = goal.TeamId == Match.HomeTeamId,
                    Minute = goal.Minute,
                    DomId = $"ev-goal-{goal.Id}",
                    Player = goal.PlayerName,
                    Meta = meta,
                    Score = $"{goal.HomeScoreAfter}-{goal.AwayScoreAfter}",
                    AriaLabel = $"{MinuteText(goal.Minute)}. dakika, {goal.TeamName} golü, {goal.PlayerName}",
                    Title = $"{MinuteLabel(goal.Minute)} Gol: {goal.PlayerName}"
                });
            }

            foreach (var card in Match.Cards)
            {
                bool isRed = card.CardType == CardType.Red;
                var word = isRed ? "Kırmızı kart" : "Sarı kart";

                events.Add(new MatchEventViewModel
                {
                    Kind = "card",
                    Variant = isRed ? "red" : "yellow",
                    IsHome = card.TeamId == Match.HomeTeamId,
                    Minute = card.Minute,
                    DomId = $"ev-card-{card.Id}",
                    Player = card.PlayerName,
                    Meta = string.IsNullOrWhiteSpace(card.Reason) ? word : $"{word}, {card.Reason}",
                    AriaLabel = $"{MinuteText(card.Minute)}. dakika, {word.ToLower(Turkish)}, {card.PlayerName}",
                    Title = $"{MinuteLabel(card.Minute)} {word}: {card.PlayerName}"
                });
            }

            foreach (var sub in Match.Substitutions)
            {
                events.Add(new MatchEventViewModel
                {
                    Kind = "sub",
                    Variant = "sub",
                    IsHome = sub.TeamId == Match.HomeTeamId,
                    Minute = sub.Minute,
                    DomId = $"ev-sub-{sub.Id}",
                    Player = sub.PlayerIn,
                    Meta = sub.PlayerOut,
                    AriaLabel = $"{MinuteText(sub.Minute)}. dakika, {sub.TeamName} oyuncu değişikliği",
                    Title = $"{MinuteLabel(sub.Minute)} Değişiklik: {sub.PlayerIn} girdi"
                });
            }

            events = events.OrderBy(item => item.Minute).ToList();

            // Aynı tarafta 2 dakikadan yakın iki olay şeritte üst üste binmesin diye ikincisi biraz kaydırılır
            foreach (var isHome in new[] { true, false })
            {
                MatchEventViewModel? previous = null;

                foreach (var current in events.Where(item => item.IsHome == isHome))
                {
                    current.CloseToPrevious = previous != null && current.Minute - previous.Minute <= 2;
                    previous = current;
                }
            }

            return events;
        }

        // Şerit 0-95. dakika arasını gösterir; daha geç olaylar sağ uca yapışır
        public string FlowStyle(MatchEventViewModel e)
        {
            var style = $"--m:{Math.Min(e.Minute, 95)}";
            return e.CloseToPrevious ? style + ";margin-left:7px" : style;
        }

        //  Takımlara göre listeler 

        public List<GoalViewModel> HomeGoals => Match.Goals.Where(g => g.TeamId == Match.HomeTeamId).ToList();
        public List<GoalViewModel> AwayGoals => Match.Goals.Where(g => g.TeamId == Match.AwayTeamId).ToList();

        public List<CardViewModel> HomeCards => Match.Cards.Where(c => c.TeamId == Match.HomeTeamId).ToList();
        public List<CardViewModel> AwayCards => Match.Cards.Where(c => c.TeamId == Match.AwayTeamId).ToList();

        public List<SubstitutionViewModel> HomeSubstitutions => Match.Substitutions.Where(s => s.TeamId == Match.HomeTeamId).ToList();
        public List<SubstitutionViewModel> AwaySubstitutions => Match.Substitutions.Where(s => s.TeamId == Match.AwayTeamId).ToList();

        public string CardVariant(CardViewModel card) => card.CardType == CardType.Red ? "red" : "yellow";

        public string CardShortText(CardViewModel card) => card.CardType == CardType.Red ? "Kırmızı" : "Sarı";

        //  İstatistikler 

        public int HomeYellowCards => Match.Cards.Count(c => c.TeamId == Match.HomeTeamId && c.CardType == CardType.Yellow);
        public int AwayYellowCards => Match.Cards.Count(c => c.TeamId == Match.AwayTeamId && c.CardType == CardType.Yellow);
        public int HomeRedCards => Match.Cards.Count(c => c.TeamId == Match.HomeTeamId && c.CardType == CardType.Red);
        public int AwayRedCards => Match.Cards.Count(c => c.TeamId == Match.AwayTeamId && c.CardType == CardType.Red);

        public string PercentText(int value) => $"%{value}";

        public List<StatRowViewModel> StatRows
        {
            get
            {
                var s = Match.Statistic;

                if (s == null)
                    return new List<StatRowViewModel>();

                return new List<StatRowViewModel>
                {
                    StatRow("Şut", s.HomeShots, s.AwayShots),
                    StatRow("İsabetli şut", s.HomeShotsOnTarget, s.AwayShotsOnTarget),
                    StatRow("Pas", s.HomePasses, s.AwayPasses),
                    StatRow("Pas isabeti", s.HomePassAccuracy, s.AwayPassAccuracy, percent: true),
                    StatRow("Korner", s.HomeCorners, s.AwayCorners),
                    StatRow("Faul", s.HomeFouls, s.AwayFouls, muted: true),
                    StatRow("Ofsayt", s.HomeOffsides, s.AwayOffsides, muted: true),
                    StatRow("Sarı kart", HomeYellowCards, AwayYellowCards, muted: true),
                    StatRow("Kırmızı kart", HomeRedCards, AwayRedCards, muted: true)
                };
            }
        }

        private StatRowViewModel StatRow(string label, int home, int away, bool percent = false, bool muted = false)
        {
            int total = home + away;

            return new StatRowViewModel
            {
                Label = label,
                HomeText = percent ? PercentText(home) : home.ToString(CultureInfo.InvariantCulture),
                AwayText = percent ? PercentText(away) : away.ToString(CultureInfo.InvariantCulture),
                HomeWidth = BarWidth(home, total),
                AwayWidth = BarWidth(away, total),
                HomeLeads = home > away,
                AwayLeads = away > home,
                Muted = muted
            };
        }

        // Çubuk genişliği (CSS için nokta ondalıklı): iki çubuk arasındaki 4 px boşluk için her birinden 2 px düşülür
        private static string BarWidth(int value, int total)
        {
            if (total == 0 || value == 0) return "0%";
            if (value == total) return "100%";

            return $"calc({(value * 100.0 / total).ToString("0.#", CultureInfo.InvariantCulture)}% - 2px)";
        }

        //  Maç bilgileri 
        public string DateText => Match.MatchDate.ToString("d MMMM yyyy", Turkish);

        public string TimeText => Match.MatchDate.ToString("HH:mm", Turkish);

        public string CityText => string.IsNullOrWhiteSpace(Match.City) ? Country : $"{Match.City}, {Country}";

        public bool HasReferee => !string.IsNullOrWhiteSpace(Match.Referee);

        public string RefereeText => HasReferee ? Match.Referee! : "-";

        public string AttendanceText => Match.Attendance.HasValue ? Match.Attendance.Value.ToString("N0", Turkish) : "-";

        //  Armalar 

        public CrestViewModel HomeCrest(string sizeClass = "")
        {
            return new CrestViewModel
            {
                Code = Match.HomeTeamCode,
                ColorA = Match.HomeTeamColorA,
                ColorB = Match.HomeTeamColorB,
                LogoUrl = Match.HomeTeamLogoUrl,
                SizeClass = sizeClass
            };
        }

        public CrestViewModel AwayCrest(string sizeClass = "")
        {
            return new CrestViewModel
            {
                Code = Match.AwayTeamCode,
                ColorA = Match.AwayTeamColorA,
                ColorB = Match.AwayTeamColorB,
                LogoUrl = Match.AwayTeamLogoUrl,
                SizeClass = sizeClass
            };
        }
    }
}
