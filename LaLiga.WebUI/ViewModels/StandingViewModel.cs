namespace LaLiga.WebUI.ViewModels
{
    public class StandingViewModel
    {
        public int Position { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string ColorA { get; set; } = string.Empty;
        public string ColorB { get; set; } = string.Empty;
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference { get; set; }
        public int Points { get; set; }

        // Son 5 maç, eskiden yeniye: "W" galibiyet, "D" beraberlik, "L" mağlubiyet
        public List<string> Form { get; set; } = new();
    }
}
