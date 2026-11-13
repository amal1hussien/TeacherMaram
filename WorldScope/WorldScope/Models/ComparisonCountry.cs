using WorldScope.API.Models;

namespace WorldScope.API.Models
{
    public class ComparisonCountry
    {
        public int ComparisonId { get; set; }

        public string CountryCode { get; set; }

        public Comparison Comparison { get; set; }

        public Country Country { get; set; }
    }
}