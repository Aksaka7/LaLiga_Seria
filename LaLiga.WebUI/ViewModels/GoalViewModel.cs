namespace LaLiga.WebUI.ViewModels
{
    public class GoalViewModel
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public int Minute { get; set; }
        public string? AssistPlayerName { get; set; }
        public string? Detail { get; set; }

        // Bu gol atıldığında maçın anlık skoru
        public int HomeScoreAfter { get; set; }
        public int AwayScoreAfter { get; set; }
    }
}
