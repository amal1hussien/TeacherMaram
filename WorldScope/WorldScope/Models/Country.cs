using System.ComponentModel.DataAnnotations;
using WorldScope.API.Models;

namespace WorldScope.API.Models
{
    public class Country
    {
        [Key]
        [MaxLength(3)]
        public string CountryCode { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string? OfficialName { get; set; }

        [MaxLength(150)]
        public string? Capital { get; set; }

        [MaxLength(100)]
        public string? Region { get; set; }

        [MaxLength(100)]
        public string? Subregion { get; set; }

        public long Population { get; set; }

        public decimal? Area { get; set; }

        [MaxLength(500)]
        public string? FlagUrl { get; set; }

        public string? CurrencyData { get; set; }

        public string? LanguagesData { get; set; }

        public DateTime? LastSyncedAt { get; set; }

        public ICollection<Favorite> Favorites { get; set; }
            = new List<Favorite>();

        public ICollection<ComparisonCountry> ComparisonCountries { get; set; }
            = new List<ComparisonCountry>();
    }
}