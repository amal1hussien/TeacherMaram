using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldScope.API.Data;
using WorldScope.API.DTOs;
using WorldScope.API.Models;

namespace WorldScope.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly WorldScopeDbContext _context;

        public FavoritesController(WorldScopeDbContext context)
        {
            _context = context;
        }

        // عرض المفضلة
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var favorites = await _context.Favorites
                .Include(f => f.Country)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var result = favorites.Select(f => new FavoriteDto
            {
                FavoriteId = f.FavoriteId,
                CountryCode = f.CountryCode,
                CountryName = f.Country.Name,
                FlagUrl = f.Country.FlagUrl,
                Notes = f.Notes,
                CreatedAt = f.CreatedAt
            });

            return Ok(result);
        }

        // إضافة دولة للمفضلة
        [HttpPost]
        public async Task<IActionResult> AddFavorite(
            FavoriteCreateDto dto)
        {
            var code = dto.CountryCode.Trim().ToUpperInvariant();

            var country = await _context.Countries
                .FirstOrDefaultAsync(c => c.CountryCode == code);

            if (country == null)
                return BadRequest("Country does not exist.");

            var exists = await _context.Favorites
                .AnyAsync(f => f.CountryCode == code);

            if (exists)
                return BadRequest("Country is already in favorites.");

            var favorite = new Favorite
            {
                CountryCode = code,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Country added to favorites.",
                favoriteId = favorite.FavoriteId
            });
        }

        // تعديل ملاحظة
        [HttpPut("{id}/notes")]
        public async Task<IActionResult> UpdateNotes(
            int id,
            FavoriteNoteDto dto)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.FavoriteId == id);

            if (favorite == null)
                return NotFound("Favorite not found.");

            favorite.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Note updated successfully."
            });
        }

        // حذف من المفضلة
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavorite(int id)
        {
            var favorite = await _context.Favorites.FindAsync(id);

            if (favorite == null)
                return NotFound("Favorite not found.");

            _context.Favorites.Remove(favorite);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Favorite deleted successfully."
            });
        }
    }
}