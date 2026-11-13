using System.ComponentModel.DataAnnotations;

namespace WorldScope.API.DTOs
{
    public class FavoriteCreateDto
    {
        [Required]
        [MaxLength(3)]
        public string CountryCode { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}