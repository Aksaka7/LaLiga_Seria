namespace LaLiga.WebUI.ViewModels
{
    // Admin > Takımlar sayfasının modeli: filtreli/sıralı takım listesi + lig özeti + filtre formunun mevcut değerleri
    public class AdminTeamsPageViewModel
    {
        public List<TeamViewModel> Teams { get; set; } = new();
        public int TotalCount { get; set; }
        public int MaxActive { get; set; }

        public int ActiveCount { get; set; }
        public int CityCount { get; set; }
        public int TotalCapacity { get; set; }
        public TeamViewModel? Oldest { get; set; }

        //  Filtre formunun geri doldurulması 
        public string? FilterQuery { get; set; }
        public TeamRegion? FilterRegion { get; set; }
        public string? FilterStatus { get; set; } // "active" | "inactive" | null

        //  Sıralama 
        public string SortKey { get; set; } = "id";
        public string SortDir { get; set; } = "asc";

        public bool Empty => Teams.Count == 0;

        public string FooterText => $"{TotalCount} takımdan {Teams.Count} tanesi gösteriliyor";

        public static readonly (TeamRegion Value, string Label)[] RegionOptions =
        {
            (TeamRegion.North, "Kuzey"),
            (TeamRegion.Center, "Orta"),
            (TeamRegion.East, "Doğu"),
            (TeamRegion.South, "Güney"),
            (TeamRegion.Island, "Adalar")
        };

        public string RegionLabel(TeamRegion region) => region switch
        {
            TeamRegion.North => "Kuzey",
            TeamRegion.Center => "Orta",
            TeamRegion.East => "Doğu",
            TeamRegion.South => "Güney",
            _ => "Adalar"
        };

        // CSS sınıfı icin
        public string RegionSlug(TeamRegion region) => region.ToString().ToLowerInvariant();

        public CrestViewModel Crest(TeamViewModel t, string sizeClass = "") => new()
        {
            Code = t.Code,
            ColorA = t.ColorA,
            ColorB = t.ColorB,
            LogoUrl = t.LogoUrl,
            SizeClass = sizeClass
        };

        //  Sıralama başlıkları 

        public string AriaSort(string key) => SortKey == key ? (SortDir == "asc" ? "ascending" : "descending") : "none";
        
        public string SortUrl(string key)
        {// Sıralama başlığına tıklanınca gidilecek adres: aynı sütuna tekrar tıklarsa yön ters döner, diğer filtreler korunur
            var dir = SortKey == key && SortDir == "asc" ? "desc" : "asc";
            var query = new List<string> { $"sort={key}", $"dir={dir}" };

            if (!string.IsNullOrWhiteSpace(FilterQuery))
                query.Add($"q={Uri.EscapeDataString(FilterQuery)}");

            if (FilterRegion.HasValue)
                query.Add($"region={FilterRegion}");

            if (!string.IsNullOrWhiteSpace(FilterStatus))
                query.Add($"status={FilterStatus}");

            return "/Admin/Teams?" + string.Join("&", query);
        }
    }
}
