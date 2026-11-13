using System.ComponentModel.DataAnnotations;

namespace WorldScope.DTOs
{
    public class ComparisonCreateDto
    {
        [MaxLength(150)]
        public string? Title { get; set; }

        [Required]
        public List<string> CountryCodes { get; set; }
            = new List<string>();
    }
}