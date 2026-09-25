using LaLiga.Entities;

namespace LaLiga.DTOs
{
    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string City { get; set; } = string.Empty;
        public TeamRegion Region { get; set; }
        public string Stadium { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int FoundedYear { get; set; }
        public string ColorA { get; set; } = string.Empty;
        public string ColorB { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
