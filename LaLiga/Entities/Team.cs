using System.ComponentModel.DataAnnotations;

namespace LaLiga.Entities
{
    public class Team
    {
        public int Id { get; set; }

        [Required, MaxLength(40)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(3, MinimumLength = 3)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        public TeamRegion Region { get; set; }

        [Required, MaxLength(100)]
        public string Stadium { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public int FoundedYear { get; set; }

        [Required, MaxLength(7)]
        public string ColorA { get; set; } = "#1E5BD8";

        [Required, MaxLength(7)]
        public string ColorB { get; set; } = "#FFFFFF";

        public bool IsActive { get; set; } = true;
    }
}
