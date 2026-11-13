using WorldScope.API.DTOs;

namespace WorldScope.API.DTOs
{
    public class ComparisonDto
    {
        public int ComparisonId { get; set; }

        public string? Title { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<CountryDto> Countries { get; set; }
            = new List<CountryDto>();
    }
}