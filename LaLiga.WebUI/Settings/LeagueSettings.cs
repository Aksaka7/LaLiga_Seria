namespace LaLiga.WebUI.Settings
{
    public class LeagueSettings
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public int TotalWeeks { get; set; }
        public int TeamCount { get; set; }
        public int MaxActiveTeams { get; set; }
    }
}
