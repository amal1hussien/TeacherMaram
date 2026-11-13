using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldScope.API.Data;
using WorldScope.API.DTOs;
using WorldScope.API.Models;
using WorldScope.DTOs;

namespace WorldScope.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComparisonsController : ControllerBase
    {
        private readonly WorldScopeDbContext _context;

        public ComparisonsController(WorldScopeDbContext context)
        {
            _context = context;
        }

        // عرض جميع المقارنات
        [HttpGet]
        public async Task<IActionResult> GetComparisons()
        {
            var comparisons = await _context.Comparisons
                .Include(c => c.ComparisonCountries)
                .ThenInclude(cc => cc.Country)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var result = comparisons.Select(c => MapComparison(c));

            return Ok(result);
        }

        // عرض مقارنة محددة
        [HttpGet("{id}")]
        public async Task<IActionResult> GetComparison(int id)
        {
            var comparison = await _context.Comparisons
                .Include(c => c.ComparisonCountries)
                .ThenInclude(cc => cc.Country)
                .FirstOrDefaultAsync(c => c.ComparisonId == id);

            if (comparison == null)
                return NotFound("Comparison not found.");

            return Ok(MapComparison(comparison));
        }

        // إنشاء مقارنة جديدة
        [HttpPost]
        public async Task<IActionResult> CreateComparison(
            ComparisonCreateDto dto)
        {
            var codes = dto.CountryCodes
                .Select(c => c.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();

            if (codes.Count != 2)
                return BadRequest("Choose two different countries.");

            var countries = await _context.Countries
                .Where(c => codes.Contains(c.CountryCode))
                .ToListAsync();

            if (countries.Count != 2)
                return BadRequest("One or more countries do not exist.");

            var comparison = new Comparison
            {
                Title = string.IsNullOrWhiteSpace(dto.Title)
                    ? "New Comparison"
                    : dto.Title.Trim(),

                CreatedAt = DateTime.UtcNow,

                ComparisonCountries = codes.Select(code =>
                    new ComparisonCountry
                    {
                        CountryCode = code
                    }).ToList()
            };

            _context.Comparisons.Add(comparison);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Comparison created successfully.",
                comparisonId = comparison.ComparisonId
            });
        }

        // تعديل مقارنة
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComparison(
            int id,
            ComparisonCreateDto dto)
        {
            var comparison = await _context.Comparisons
                .Include(c => c.ComparisonCountries)
                .FirstOrDefaultAsync(c => c.ComparisonId == id);

            if (comparison == null)
                return NotFound("Comparison not found.");

            var codes = dto.CountryCodes
                .Select(c => c.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();

            if (codes.Count != 2)
                return BadRequest("Choose two different countries.");

            var countries = await _context.Countries
                .Where(c => codes.Contains(c.CountryCode))
                .ToListAsync();

            if (countries.Count != 2)
                return BadRequest("One or more countries do not exist.");

            comparison.Title = dto.Title;

            _context.ComparisonCountries.RemoveRange(
                comparison.ComparisonCountries);

            comparison.ComparisonCountries.Clear();

            foreach (var code in codes)
            {
                comparison.ComparisonCountries.Add(
                    new ComparisonCountry
                    {
                        ComparisonId = comparison.ComparisonId,
                        CountryCode = code
                    });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Comparison updated successfully."
            });
        }

        // حذف مقارنة
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComparison(int id)
        {
            var comparison = await _context.Comparisons
                .FirstOrDefaultAsync(c => c.ComparisonId == id);

            if (comparison == null)
                return NotFound("Comparison not found.");

            _context.Comparisons.Remove(comparison);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Comparison deleted successfully."
            });
        }

        // تحويل بيانات المقارنة إلى DTO
        private static ComparisonDto MapComparison(
            Comparison comparison)
        {
            return new ComparisonDto
            {
                ComparisonId = comparison.ComparisonId,
                Title = comparison.Title,
                CreatedAt = comparison.CreatedAt,

                Countries = comparison.ComparisonCountries
                    .Where(cc => cc.Country != null)
                    .Select(cc => new CountryDto
                    {
                        CountryCode = cc.Country.CountryCode,
                        Name = cc.Country.Name,
                        OfficialName = cc.Country.OfficialName,
                        Capital = cc.Country.Capital,
                        Region = cc.Country.Region,
                        Subregion = cc.Country.Subregion,
                        Population = cc.Country.Population,
                        Area = cc.Country.Area,
                        FlagUrl = cc.Country.FlagUrl,
                        CurrencyData = cc.Country.CurrencyData,
                        LanguagesData = cc.Country.LanguagesData
                    }).ToList()
            };
        }
    }
}