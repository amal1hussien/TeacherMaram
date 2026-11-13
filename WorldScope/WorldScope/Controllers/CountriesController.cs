using Microsoft.AspNetCore.Mvc;
using WorldScope.API.DTOs;
using WorldScope.API.Services;

namespace WorldScope.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryService _countryService;

        public CountriesController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        // عرض جميع الدول
        [HttpGet]
        public async Task<IActionResult> GetAllCountries()
        {
            var countries = await _countryService.GetAllCountriesAsync();

            var result = countries.Select(c => new CountryDto
            {
                CountryCode = c.CountryCode,
                Name = c.Name,
                OfficialName = c.OfficialName,
                Capital = c.Capital,
                Region = c.Region,
                Subregion = c.Subregion,
                Population = c.Population,
                Area = c.Area,
                FlagUrl = c.FlagUrl,
                CurrencyData = c.CurrencyData,
                LanguagesData = c.LanguagesData
            });

            return Ok(result);
        }

        // عرض دولة باستخدام رمزها
        [HttpGet("{code}")]
        public async Task<IActionResult> GetCountry(string code)
        {
            var country = await _countryService.GetCountryByCodeAsync(code);

            if (country == null)
                return NotFound("Country not found.");

            var result = new CountryDto
            {
                CountryCode = country.CountryCode,
                Name = country.Name,
                OfficialName = country.OfficialName,
                Capital = country.Capital,
                Region = country.Region,
                Subregion = country.Subregion,
                Population = country.Population,
                Area = country.Area,
                FlagUrl = country.FlagUrl,
                CurrencyData = country.CurrencyData,
                LanguagesData = country.LanguagesData
            };

            return Ok(result);
        }

        // البحث عن دولة بالاسم
        [HttpGet("search")]
        public async Task<IActionResult> SearchCountries(
            [FromQuery] string name)
        {
            var countries = await _countryService.GetAllCountriesAsync();

            var result = countries
                .Where(c => c.Name.Contains(
                    name ?? "",
                    StringComparison.OrdinalIgnoreCase))
                .Select(c => new CountryDto
                {
                    CountryCode = c.CountryCode,
                    Name = c.Name,
                    Capital = c.Capital,
                    Region = c.Region,
                    Population = c.Population,
                    Area = c.Area,
                    FlagUrl = c.FlagUrl
                });

            return Ok(result);
        }

        // عرض الدول حسب المنطقة
        [HttpGet("region/{region}")]
        public async Task<IActionResult> GetByRegion(string region)
        {
            var countries = await _countryService.GetAllCountriesAsync();

            var result = countries
                .Where(c => c.Region != null &&
                    c.Region.Equals(
                        region,
                        StringComparison.OrdinalIgnoreCase))
                .Select(c => new CountryDto
                {
                    CountryCode = c.CountryCode,
                    Name = c.Name,
                    Capital = c.Capital,
                    Region = c.Region,
                    Population = c.Population,
                    Area = c.Area,
                    FlagUrl = c.FlagUrl
                });

            return Ok(result);
        }

        // تحديث بيانات الدول من REST Countries
        [HttpPost("sync")]
        public async Task<IActionResult> SyncCountries()
        {
            var count = await _countryService.SyncCountriesAsync();

            return Ok(new
            {
                message = "Countries synchronized successfully.",
                totalCountries = count
            });
        }
    }
}