using System.Globalization;

namespace LaLiga.WebUI.ViewModels
{
    // Takım armasını çizmek için gereken bilgiler (_TeamCrest partial'ı kullanır).
    public class CrestViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string ColorA { get; set; } = "#1E5BD8";
        public string ColorB { get; set; } = "#FFFFFF";
        public string? LogoUrl { get; set; }

        // Ek boyut sınıfı: "" (varsayılan), "team-crest--sm", "team-crest--lg" gibi
        public string SizeClass { get; set; } = string.Empty;
        public string Ink
        {
            get
            {
                var hex = ColorA.TrimStart('#');

                if (hex.Length != 6 || !int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
                    return "#FFFFFF";

                int r = (rgb >> 16) & 0xFF;
                int g = (rgb >> 8) & 0xFF;
                int b = rgb & 0xFF;

                return (0.299 * r + 0.587 * g + 0.114 * b) > 170 ? "#10224A" : "#FFFFFF";
            }
        }
    }
}
