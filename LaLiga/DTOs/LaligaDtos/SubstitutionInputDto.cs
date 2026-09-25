using System.ComponentModel.DataAnnotations;

namespace LaLiga.DTOs
{
    public class SubstitutionInputDto
    {
        public int TeamId { get; set; }

        [Required, MaxLength(100)]
        public string PlayerIn { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string PlayerOut { get; set; } = string.Empty;

        [Range(1, 130)]
        public int Minute { get; set; }
    }
}
