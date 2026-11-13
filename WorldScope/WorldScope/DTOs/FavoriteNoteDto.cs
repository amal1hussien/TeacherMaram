using System.ComponentModel.DataAnnotations;

namespace WorldScope.API.DTOs
{
    public class FavoriteNoteDto
    {
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}