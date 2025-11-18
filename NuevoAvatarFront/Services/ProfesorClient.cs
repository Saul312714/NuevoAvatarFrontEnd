using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;

namespace NuevoAvatarFront.Services
{
    public class ProfesorClient : ApiClientBase
    {
        private readonly HttpClient _http;
        private readonly ILogger<ProfesorClient> _logger;
        private readonly IConfiguration _cfg;

        public ProfesorClient(
            HttpClient http,
            ILogger<ProfesorClient> logger,
            IHttpContextAccessor httpCtx,
            IConfiguration cfg,
            TokenHelper tokenHelper
        ) : base(httpCtx, tokenHelper)
        {
            _http = http;
            _logger = logger;
            _cfg = cfg;
        }

        // 🔥 Nuevo sistema de bearer token
        private async Task AttachBearerAsync()
        {
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
                token = _cfg["Auth:DevBearerToken"]; // fallback de desarrollo

            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _http.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<(IEnumerable<ProfesorDto> datos, int total)> GetAllAsync(string? q, string? sort)
        {
            await AttachBearerAsync();

            var url = "/profesor";

            var list =
                await _http.GetFromJsonAsync<List<ProfesorDto>>(url)
                ?? new();

            return (list, list.Count);
        }

        public async Task<ProfesorDto?> GetByIdAsync(int id)
        {
            await AttachBearerAsync();

            var resp = await _http.GetAsync($"/profesor/{id}");

            if (!resp.IsSuccessStatusCode)
                return null;

            return await resp.Content.ReadFromJsonAsync<ProfesorDto>();
        }

        public async Task<bool> CreateAsync(ProfesorDto model)
        {
            await AttachBearerAsync();

            var resp = await _http.PostAsJsonAsync("/profesor", model);

            if (!resp.IsSuccessStatusCode)
                _logger.LogWarning("Create profesor failed: {Status}", resp.StatusCode);

            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(ProfesorDto model)
        {
            await AttachBearerAsync();

            var resp = await _http.PutAsJsonAsync("/profesor", model);

            if (!resp.IsSuccessStatusCode)
                _logger.LogWarning("Update profesor failed: {Status}", resp.StatusCode);

            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await AttachBearerAsync();

            var resp = await _http.DeleteAsync($"/profesor/{id}");

            if (!resp.IsSuccessStatusCode)
                _logger.LogWarning("Delete profesor failed: {Status}", resp.StatusCode);

            return resp.IsSuccessStatusCode;
        }
    }
}
