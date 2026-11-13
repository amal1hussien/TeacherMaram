
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Net.Http.Headers;
using WorldScope.API.Data;
using WorldScope.API.Models;

namespace WorldScope.API.Services
{
    public class CountryService : ICountryService
    {
        private readonly WorldScopeDbContext _context;
        private readonly HttpClient _httpClient;

        private const string BaseUrl =
            "https://api.restcountries.com/countries/v5";

        // ضعي مفتاح API الخاص بك هنا
        private const string ApiKey = "rc_live_794c74fbc3b548dd8e2ef433fe59293b";

        public CountryService(
            WorldScopeDbContext context,
            HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        // =========================================
        // 1. جلب جميع الدول من قاعدة البيانات
        // =========================================
        public async Task<List<Country>> GetAllCountriesAsync()
        {
            var countries = await _context.Countries
                .OrderBy(c => c.Name)
                .ToListAsync();

            // إذا كانت قاعدة البيانات فارغة
            // نجلب الدول من REST Countries API
            if (countries.Count == 0)
            {
                await SyncCountriesAsync();

                countries = await _context.Countries
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }

            return countries;
        }

        // =========================================
        // 2. البحث عن دولة باستخدام رمزها
        // =========================================
        public async Task<Country?> GetCountryByCodeAsync(string code)
        {
            code = code.Trim().ToUpperInvariant();

            return await _context.Countries
                .FirstOrDefaultAsync(c => c.CountryCode == code);
        }

        // =========================================
        // 3. مزامنة الدول من REST Countries API v5
        // =========================================
        public async Task<int> SyncCountriesAsync()
        {
            int count = 0;
            int offset = 0;
            const int limit = 100;

            var existingCountries = await _context.Countries
                .ToDictionaryAsync(c => c.CountryCode);

            while (true)
            {
                // رابط الإصدار الخامس
                var url = $"{BaseUrl}?limit={limit}&offset={offset}";

                using var request = new HttpRequestMessage(
                    HttpMethod.Get, url);

                // إضافة مفتاح API
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", ApiKey);

                // إرسال الطلب
                using var response = await _httpClient.SendAsync(request);

                var json = await response.Content.ReadAsStringAsync();

                // التأكد من نجاح الاتصال
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        $"REST Countries API Error: " +
                        $"{(int)response.StatusCode}. Response: {json}");
                }

                using var document = JsonDocument.Parse(json);

                var root = document.RootElement;

                // قراءة البيانات الرئيسية
                if (!root.TryGetProperty("data", out var data) ||
                    data.ValueKind != JsonValueKind.Object)
                {
                    throw new Exception(
                        $"Invalid API response: {json}");
                }

                // قراءة مصفوفة الدول
                if (!data.TryGetProperty("objects", out var countriesArray) ||
                    countriesArray.ValueKind != JsonValueKind.Array)
                {
                    throw new Exception(
                        $"Countries array not found: {json}");
                }

                // المرور على جميع الدول في الصفحة الحالية
                foreach (var item in countriesArray.EnumerateArray())
                {
                    // قراءة رمز الدولة
                    if (!item.TryGetProperty("codes", out var codes))
                        continue;

                    if (!codes.TryGetProperty("alpha_2", out var codeElement))
                        continue;

                    var code = codeElement.GetString();

                    if (string.IsNullOrWhiteSpace(code))
                        continue;

                    // البحث عن الدولة في قاعدة البيانات
                    if (!existingCountries.TryGetValue(code, out var country))
                    {
                        country = new Country
                        {
                            CountryCode = code
                        };

                        existingCountries.Add(code, country);

                        _context.Countries.Add(country);
                    }

                    // الاسم الشائع
                    country.Name =
                        GetNestedString(item, "names", "common") ?? code;

                    // الاسم الرسمي
                    country.OfficialName =
                        GetNestedString(item, "names", "official");

                    // العاصمة
                    country.Capital = GetCapitalName(item);

                    // المنطقة
                    country.Region = GetString(item, "region");

                    // المنطقة الفرعية
                    country.Subregion = GetString(item, "subregion");

                    // عدد السكان
                    country.Population = GetLong(item, "population");

                    // المساحة بالكيلومتر المربع
                    country.Area = GetNestedDecimal(
                        item, "area", "kilometers");

                    // رابط العلم
                    country.FlagUrl = GetNestedString(
                        item, "flag", "url_png");

                    // العملات
                    country.CurrencyData =
                        GetRawJson(item, "currencies");

                    // اللغات
                    country.LanguagesData =
                        GetRawJson(item, "languages");

                    // تاريخ آخر مزامنة
                    country.LastSyncedAt = DateTime.UtcNow;

                    count++;
                }

                // معرفة هل توجد صفحات أخرى
                bool hasMore = false;

                if (data.TryGetProperty("meta", out var meta) &&
                    meta.TryGetProperty("more", out var moreElement))
                {
                    hasMore =
                        moreElement.ValueKind == JsonValueKind.True;
                }

                // إذا انتهت الصفحات نخرج من الحلقة
                if (!hasMore)
                    break;

                offset += limit;
            }

            // حفظ البيانات في SQL Server
            await _context.SaveChangesAsync();

            return count;
        }

        // =========================================
        // 4. قراءة قيمة نصية
        // =========================================
        private static string? GetString(
            JsonElement element,
            string property)
        {
            if (element.ValueKind == JsonValueKind.Object &&
                element.TryGetProperty(property, out var value) &&
                value.ValueKind == JsonValueKind.String)
            {
                return value.GetString();
            }

            return null;
        }

        // =========================================
        // 5. قراءة قيمة نصية من كائن داخلي
        // مثال: names.common
        // =========================================
        private static string? GetNestedString(
            JsonElement element,
            string parent,
            string child)
        {
            if (element.ValueKind == JsonValueKind.Object &&
                element.TryGetProperty(parent, out var parentValue) &&
                parentValue.ValueKind == JsonValueKind.Object &&
                parentValue.TryGetProperty(child, out var childValue) &&
                childValue.ValueKind == JsonValueKind.String)
            {
                return childValue.GetString();
            }

            return null;
        }

        // =========================================
        // 6. قراءة اسم العاصمة من capitals
        // =========================================
        private static string? GetCapitalName(JsonElement element)
        {
            if (element.TryGetProperty("capitals", out var capitals) &&
                capitals.ValueKind == JsonValueKind.Array &&
                capitals.GetArrayLength() > 0)
            {
                var firstCapital = capitals[0];

                if (firstCapital.ValueKind == JsonValueKind.Object &&
                    firstCapital.TryGetProperty("name", out var name) &&
                    name.ValueKind == JsonValueKind.String)
                {
                    return name.GetString();
                }
            }

            return null;
        }

        // =========================================
        // 7. قراءة عدد صحيح
        // =========================================
        private static long GetLong(
            JsonElement element,
            string property)
        {
            if (element.TryGetProperty(property, out var value) &&
                value.ValueKind == JsonValueKind.Number &&
                value.TryGetInt64(out long result))
            {
                return result;
            }

            return 0;
        }

        // =========================================
        // 8. قراءة رقم عشري من كائن داخلي
        // مثال: area.kilometers
        // =========================================
        private static decimal? GetNestedDecimal(
            JsonElement element,
            string parent,
            string child)
        {
            if (element.TryGetProperty(parent, out var parentValue) &&
                parentValue.ValueKind == JsonValueKind.Object &&
                parentValue.TryGetProperty(child, out var childValue) &&
                childValue.ValueKind == JsonValueKind.Number &&
                childValue.TryGetDecimal(out decimal result))
            {
                return result;
            }

            return null;
        }

        // =========================================
        // 9. الاحتفاظ بالبيانات بصيغة JSON
        // مثال: currencies و languages
        // =========================================
        private static string? GetRawJson(
            JsonElement element,
            string property)
        {
            if (element.TryGetProperty(property, out var value) &&
                value.ValueKind != JsonValueKind.Null)
            {
                return value.GetRawText();
            }

            return null;
        }
    }
}
