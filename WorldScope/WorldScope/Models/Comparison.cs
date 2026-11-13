using System.ComponentModel.DataAnnotations;
using WorldScope.API.Models;

namespace WorldScope.API.Models
{
    public class Comparison
    {
        [Key]
        public int ComparisonId { get; set; }

        [MaxLength(150)]
        public string? Title { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<ComparisonCountry> ComparisonCountries
        { get; set; } = new List<ComparisonCountry>();
    }
}