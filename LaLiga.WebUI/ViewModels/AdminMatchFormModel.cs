namespace LaLiga.WebUI.ViewModels
{
    public class AdminMatchFormModel
    {
        public int? Id { get; set; }
        public int Home { get; set; }
        public int Away { get; set; }
        public int Week { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Stadium { get; set; } = string.Empty;
        public MatchStatus Status { get; set; }
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }



        //formda olmayan, düzenlemede gizli alanlarla korunan bilgiler
        public int? Minute { get; set; }
        public string? Note { get; set; }
        public string? Referee { get; set; }
        public int? Attendance { get; set; }
    }
}
