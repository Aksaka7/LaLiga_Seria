using System.ComponentModel.DataAnnotations;
using LaLiga.Entities;

namespace LaLiga.DTOs
{
    public class TeamInputDto
    {
        [Required, StringLength(40, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required, RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "Kısa ad tam olarak 3 harften oluşmalı.")]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [EnumDataType(typeof(TeamRegion))]
        public TeamRegion Region { get; set; }

        [Required, MaxLength(100)]
        public string Stadium { get; set; } = string.Empty;

        [Range(1000, 100000)]
        public int Capacity { get; set; }

        [Range(1850, 2026)]
        public int FoundedYear { get; set; }

        [Required, RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Renk #RRGGBB biçiminde olmalı.")]
        public string ColorA { get; set; } = string.Empty;

        [Required, RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Renk #RRGGBB biçiminde olmalı.")]
        public string ColorB { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
