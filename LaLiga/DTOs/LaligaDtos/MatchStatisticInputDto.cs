using System.ComponentModel.DataAnnotations;

namespace LaLiga.DTOs
{
    // Deplasmanın topa sahip olma oranı = 100 - HomePossession (ayrıca girilmez)
    public class MatchStatisticInputDto
    {
        [Range(0, 100)] public int HomePossession { get; set; }

        [Range(0, 100)] public int HomeShots { get; set; }
        [Range(0, 100)] public int AwayShots { get; set; }

        [Range(0, 100)] public int HomeShotsOnTarget { get; set; }
        [Range(0, 100)] public int AwayShotsOnTarget { get; set; }

        [Range(0, 2000)] public int HomePasses { get; set; }
        [Range(0, 2000)] public int AwayPasses { get; set; }

        [Range(0, 100)] public int HomePassAccuracy { get; set; }
        [Range(0, 100)] public int AwayPassAccuracy { get; set; }

        [Range(0, 50)] public int HomeCorners { get; set; }
        [Range(0, 50)] public int AwayCorners { get; set; }

        [Range(0, 100)] public int HomeFouls { get; set; }
        [Range(0, 100)] public int AwayFouls { get; set; }

        [Range(0, 50)] public int HomeOffsides { get; set; }
        [Range(0, 50)] public int AwayOffsides { get; set; }
    }
}
