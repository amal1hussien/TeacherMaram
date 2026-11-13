using System.ComponentModel.DataAnnotations;
using WorldScope.API.Models;

namespace WorldScope.API.Models
{
    public class Favorite
    {
        [Key]
        public int FavoriteId { get; set; }

        [Required]
        [MaxLength(3)]
        public string CountryCode { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public Country Country { get; set; }
    }
}