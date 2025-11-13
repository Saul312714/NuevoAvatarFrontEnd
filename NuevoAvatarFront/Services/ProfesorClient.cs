using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Models;

namespace NuevoAvatarFront.Services
{
    public class ProfesorClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<ProfesorClient> _logger;
        private readonly IHttpContextAccessor _httpCtx;
        private readonly IConfiguration _cfg;

        public ProfesorClient(HttpClient http, ILogger<ProfesorClient> logger, IHttpContextAccessor httpCtx, IConfiguration cfg)
        {
            _http = http;
            _logger = logger;
            _httpCtx = httpCtx;
            _cfg = cfg;
        }

        private void AttachBearer()
        {
            var token = _httpCtx.HttpContext?.Session.GetString("JWT");

            if (string.IsNullOrWhiteSpace(token))
                token = _cfg["Auth:DevBearerToken"];

            if (!string.IsNullOrWhiteSpace(token))
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            else
                _http.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<(IEnumerable<ProfesorDto> datos, int total)> GetAllAsync(string? q, string? sort)
        {
            AttachBearer();
            var url = "/profesor";
            var list = await _http.GetFromJsonAsync<List<ProfesorDto>>(url) ?? new();
            return (list, list.Count);
        }

        public async Task<ProfesorDto?> GetByIdAsync(int id)
        {
            AttachBearer();
            var resp = await _http.GetAsync($"/profesor/{id}");
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ProfesorDto>();
        }

        public async Task<bool> CreateAsync(ProfesorDto model)
        {
            AttachBearer();
            var resp = await _http.PostAsJsonAsync("/profesor", model);
            if (!resp.IsSuccessStatusCode)
                _logger.LogWarning("Create profesor failed: {Status}", resp.StatusCode);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(ProfesorDto model)
        {
            AttachBearer();
            var resp = await _http.PutAsJsonAsync("/profesor", model);
            if (!resp.IsSuccessStatusCode)
                _logger.LogWarning("Update profesor failed: {Status}", resp.StatusCode);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            AttachBearer();
            var resp = await _http.DeleteAsync($"/profesor/{id}");
            if (!resp.IsSuccessStatusCode)
                _logger.LogWarning("Delete profesor failed: {Status}", resp.StatusCode);
            return resp.IsSuccessStatusCode;
        }
    }
}