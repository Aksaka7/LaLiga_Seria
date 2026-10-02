namespace LaLiga.WebUI.ViewModels
{
    // Takım ekle/düzenle modalının form alanlarına birebir karşılık gelir
    public class AdminTeamFormModel
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public TeamRegion Region { get; set; }
        public string Stadium { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int Founded { get; set; }
        public string ColorA { get; set; } = string.Empty;
        public string ColorB { get; set; } = string.Empty;
        public string Status { get; set; } = "active";  
    }
}
