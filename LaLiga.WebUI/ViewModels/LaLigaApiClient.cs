using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LaLiga.WebUI.ViewModels;

namespace LaLiga.WebUI.Services
{
    // WebUI'nin API ile konuşan tek yeri. Sayfalar HttpClient kullanmaz, bu sınıfı kullanır.
    public class LaLigaApiClient
    {
        // API enum'ları yazı olarak gönderir ("Live", "Finished"); alan adları camelCase
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly HttpClient _http;

        public LaLigaApiClient(HttpClient http)
        {
            _http = http;
        }

        public Task<List<StandingViewModel>> GetStandingsAsync()
        {
            return GetAsync<List<StandingViewModel>>("api/standings");
        }

        public Task<List<WeekViewModel>> GetWeeksAsync()
        {
            return GetAsync<List<WeekViewModel>>("api/weeks");
        }

        public Task<List<MatchViewModel>> GetMatchesByWeekAsync(int week)
        {
            return GetAsync<List<MatchViewModel>>($"api/matches/week/{week}");
        }

        public Task<MatchDetailViewModel> GetMatchAsync(int id)
        {
            return GetAsync<MatchDetailViewModel>($"api/matches/{id}");
        }

        // Admin listesi ve filtreler: hafta, durum, takım adı
        public Task<List<MatchViewModel>> GetMatchesAsync(int? week = null, MatchStatus? status = null, string? team = null)
        {
            var query = new List<string>();

            if (week.HasValue)
                query.Add($"week={week.Value}");

            if (status.HasValue)
                query.Add($"status={status.Value}");

            if (!string.IsNullOrWhiteSpace(team))
                query.Add($"team={Uri.EscapeDataString(team.Trim())}");

            var url = query.Count == 0 ? "api/matches" : "api/matches?" + string.Join("&", query);
            return GetAsync<List<MatchViewModel>>(url);
        }

        private async Task<T> GetAsync<T>(string url)
        {
            HttpResponseMessage response;

            try
            {
                response = await _http.GetAsync(url);
            }
            catch (HttpRequestException)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable,
                    "API'ye ulaşılamıyor. API projesinin (LaLiga) çalıştığından emin olun.");
            }
            catch (TaskCanceledException)
            {
                throw new ApiException(HttpStatusCode.GatewayTimeout, "API zamanında yanıt vermedi.");
            }

            using (response)
            {
                await EnsureSuccessAsync(response);

                var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
                return value ?? throw new ApiException(HttpStatusCode.BadGateway, "API'den boş yanıt geldi.");
            }
        }

        // Başarısız yanıtı { message, errors } biçiminden ApiException'a çevirir
        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            ApiErrorResponse? error = null;

            try
            {
                error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
            }
            catch (JsonException)
            {
                // API JSON dışında bir şey döndürdü (örn. HTML hata sayfası)
            }
            catch (NotSupportedException)
            {
                // Yanıtın içerik türü JSON değil
            }

            throw new ApiException(
                response.StatusCode,
                error?.Message ?? $"API isteği başarısız oldu ({(int)response.StatusCode}).",
                error?.Errors);
        }
    }
}
