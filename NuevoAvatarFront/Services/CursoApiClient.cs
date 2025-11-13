using System.Net.Http.Json;
using NuevoAvatarFront.DTOs;

namespace NuevoAvatarFront.Services
{
    public class CursoApiClient : ICursoApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _context;

        public CursoApiClient(HttpClient http, IHttpContextAccessor context)
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

        public async Task<List<CursoDto>> GetByCarreraAndNivelAsync(int carreraId, int? nivel)
        {
            AddToken();
            var url = $"curso?carreraId={carreraId}";
            if (nivel.HasValue)
                url += $"&nivel={nivel.Value}";

            var resp = await _http.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
                return new List<CursoDto>();

            return await resp.Content.ReadFromJsonAsync<List<CursoDto>>() ?? new List<CursoDto>();
        }

        public async Task<CursoDto?> GetByIdAsync(int id)
        {
            AddToken();
            return await _http.GetFromJsonAsync<CursoDto>($"curso/{id}");
        }

        public async Task<bool> CreateAsync(CursoCreateUpdateDto dto)
        {
            AddToken();
            var resp = await _http.PostAsJsonAsync("curso", dto);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int id, CursoCreateUpdateDto dto)
        {
            AddToken();
            var resp = await _http.PutAsJsonAsync($"curso/{id}", dto);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            AddToken();
            var resp = await _http.DeleteAsync($"curso/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}
