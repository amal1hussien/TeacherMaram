namespace WorldScope.API.DTOs
{
    public class FavoriteDto
    {
        public int FavoriteId { get; set; }

        public string CountryCode { get; set; } = string.Empty;

        public string CountryName { get; set; } = string.Empty;

        public string? FlagUrl { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}