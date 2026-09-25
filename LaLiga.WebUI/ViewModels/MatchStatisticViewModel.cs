namespace LaLiga.WebUI.ViewModels
{
    public class MatchStatisticViewModel
    {
        public int HomePossession { get; set; }
        public int AwayPossession { get; set; }

        public int HomeShots { get; set; }
        public int AwayShots { get; set; }

        public int HomeShotsOnTarget { get; set; }
        public int AwayShotsOnTarget { get; set; }

        public int HomePasses { get; set; }
        public int AwayPasses { get; set; }

        public int HomePassAccuracy { get; set; }
        public int AwayPassAccuracy { get; set; }

        public int HomeCorners { get; set; }
        public int AwayCorners { get; set; }

        public int HomeFouls { get; set; }
        public int AwayFouls { get; set; }

        public int HomeOffsides { get; set; }
        public int AwayOffsides { get; set; }
    }
}
