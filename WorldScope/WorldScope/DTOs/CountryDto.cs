namespace WorldScope.API.DTOs
{
    public class CountryDto
    {
        public string CountryCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? OfficialName { get; set; }

        public string? Capital { get; set; }

        public string? Region { get; set; }

        public string? Subregion { get; set; }

        public long Population { get; set; }

        public decimal? Area { get; set; }

        public string? FlagUrl { get; set; }

        public string? CurrencyData { get; set; }

        public string? LanguagesData { get; set; }
    }
}