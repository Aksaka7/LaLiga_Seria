namespace LaLiga.WebUI.ViewModels
{
    public class CardViewModel
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public int Minute { get; set; }
        public CardType CardType { get; set; }
        public string? Reason { get; set; }
    }
}
