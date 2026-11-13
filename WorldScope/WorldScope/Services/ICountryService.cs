using WorldScope.API.Models;
using WorldScope.API.Models;

namespace WorldScope.API.Services
{
    public interface ICountryService
    {
        Task<List<Country>> GetAllCountriesAsync();

        Task<Country?> GetCountryByCodeAsync(string code);

        Task<int> SyncCountriesAsync();
    }
}