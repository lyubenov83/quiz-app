using System.Net.Http.Json;
using System.Text.Json;
using QuizClient.Models;

namespace QuizClient.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly JsonSerializerOptions _json;

        public ApiClient(string baseUrl, string apiKey)
        {
            _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _apiKey = apiKey;

            _json = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _json.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        }

        // Public endpoint: no API key
        public async Task<List<string>> GetCategoriesAsync()
        {
            var resp = await _http.GetAsync("/api/categories");
            resp.EnsureSuccessStatusCode();
            var data = await resp.Content.ReadFromJsonAsync<List<string>>(_json);
            return data ?? new List<string>();
        }

        // Protected
        public async Task<List<Question>> GetNormalAsync(IEnumerable<string> categories, int count)
        {
            var catParam = string.Join(",", categories.Select(Uri.EscapeDataString));
            var url = $"/api/questions/normal?categories={catParam}&count={count}";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("X-API-KEY", _apiKey);

            var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            var list = await resp.Content.ReadFromJsonAsync<List<Question>>(_json);
            return list ?? new List<Question>();
        }

        // Protected
        public async Task<List<Question>> GetSurvivalAsync(int total)
        {
            var url = $"/api/questions/survival?total={total}";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("X-API-KEY", _apiKey);

            var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            var list = await resp.Content.ReadFromJsonAsync<List<Question>>(_json);
            return list ?? new List<Question>();
        }
    }
}
