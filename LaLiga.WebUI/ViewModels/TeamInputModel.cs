namespace LaLiga.WebUI.ViewModels
{
    // API'ye takım eklerken ya da güncellerken gönderilen gövde (API'deki TeamInputDto ile aynı alanlar)
    public class TeamInputModel
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string City { get; set; } = string.Empty;
        public TeamRegion Region { get; set; }
        public string Stadium { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int FoundedYear { get; set; }
        public string ColorA { get; set; } = string.Empty;
        public string ColorB { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
