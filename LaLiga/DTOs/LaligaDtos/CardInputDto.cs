using System.ComponentModel.DataAnnotations;
using LaLiga.Entities;

namespace LaLiga.DTOs
{
    public class CardInputDto
    {
        public int TeamId { get; set; }

        [Required, MaxLength(100)]
        public string PlayerName { get; set; } = string.Empty;

        [Range(1, 130)]
        public int Minute { get; set; }

        [EnumDataType(typeof(CardType))]
        public CardType CardType { get; set; }

        // Örn. "faul", "itiraz", "oyunu geciktirme"
        [MaxLength(100)]
        public string? Reason { get; set; }
    }
}
