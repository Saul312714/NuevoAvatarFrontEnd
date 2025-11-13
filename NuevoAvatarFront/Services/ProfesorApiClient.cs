using System.Net.Http.Json;
using NuevoAvatarFront.DTOs;

namespace NuevoAvatarFront.Services
{
    public interface IProfesorApiClient
    {
        Task<List<ProfesorDto>> SearchAsync(string term);
    }

    public class ProfesorApiClient : IProfesorApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _context;

        public ProfesorApiClient(HttpClient http, IHttpContextAccessor context)
        {
            _http = http;
            _context = context;
        }

        private void AddToken()
        {
            var token = _context.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<ProfesorDto>> SearchAsync(string term)
        {
            AddToken();
            var resp = await _http.GetAsync($"profesor/search?q={Uri.EscapeDataString(term)}");
            if (!resp.IsSuccessStatusCode)
                return new();

            return await resp.Content.ReadFromJsonAsync<List<ProfesorDto>>() ?? new();
        }
    }
}
