using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LaLiga.WebUI.ViewModels;

namespace LaLiga.WebUI.Services
{
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

        //  Okuma 

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

        // Takımlar (ada göre sıralı). activeOnly: yalnızca aktif takımlar (maç formundaki seçim listeleri için)
        public Task<List<TeamViewModel>> GetTeamsAsync(bool activeOnly = false)
        {
            return GetAsync<List<TeamViewModel>>(activeOnly ? "api/teams?activeOnly=true" : "api/teams");
        }

        public Task<TeamViewModel> GetTeamAsync(int id)
        {
            return GetAsync<TeamViewModel>($"api/teams/{id}");
        }

        //  Maç yazma 

        // API 201 ile oluşan maçı döndürür (Id buradan alınır)
        public async Task<MatchViewModel> CreateMatchAsync(MatchInputModel input)
        {
            using var response = await SendAsync(HttpMethod.Post, "api/matches", input);
            return await ReadAsync<MatchViewModel>(response);
        }

        // API 204 döner, gövde yok
        public async Task UpdateMatchAsync(int id, MatchInputModel input)
        {
            using var response = await SendAsync(HttpMethod.Put, $"api/matches/{id}", input);
        }

        public async Task DeleteMatchAsync(int id)
        {
            using var response = await SendAsync(HttpMethod.Delete, $"api/matches/{id}");
        }

        //  Ortak yardımcılar 

        private async Task<T> GetAsync<T>(string url)
        {
            using var response = await SendAsync(HttpMethod.Get, url);
            return await ReadAsync<T>(response);
        }

        // Başarılı yanıtı çağıran alır ve kendisi kapatır (using).
        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null)
        {
            HttpResponseMessage response;

            try
            {
                using var request = new HttpRequestMessage(method, url);

                if (body != null)
                    request.Content = JsonContent.Create(body, body.GetType(), options: JsonOptions);

                response = await _http.SendAsync(request);
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

            try
            {
                await EnsureSuccessAsync(response);
            }
            catch
            {
                response.Dispose();
                throw;
            }

            return response;
        }

        private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
        {
            var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            return value ?? throw new ApiException(HttpStatusCode.BadGateway, "API'den boş yanıt geldi.");
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
                // API JSON dışında bir şey döndürdü  HTML hata sayfası
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
