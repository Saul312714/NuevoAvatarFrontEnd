using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;

namespace NuevoAvatarFront.Services
{
    public class PeriodoClient : ApiClientBase
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _cfg;
        private readonly ILogger<PeriodoClient> _logger;

        public PeriodoClient(
            HttpClient http,
            IHttpContextAccessor httpCtx,
            IConfiguration cfg,
            TokenHelper tokenHelper,
            ILogger<PeriodoClient> logger
        ) : base(httpCtx, tokenHelper)
        {
            _http = http;
            _cfg = cfg;
            _logger = logger;
        }

        
        private async Task AttachBearerAsync()
        {
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                token = _cfg["Auth:DevBearerToken"];
            }

            _http.DefaultRequestHeaders.Authorization =
                string.IsNullOrWhiteSpace(token)
                    ? null
                    : new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<Models.PeriodoDto>> GetAllAsync()
        {
            await AttachBearerAsync();

            using var resp = await _http.GetAsync("/periodo");
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<List<Models.PeriodoDto>>() ?? new();
        }

        public async Task<Models.PeriodoDto?> GetByIdAsync(int id)
        {
            await AttachBearerAsync();

            using var resp = await _http.GetAsync($"/periodo/{id}");
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadFromJsonAsync<Models.PeriodoDto?>();
        }

        public async Task<bool> CreateAsync(Models.PeriodoUpsertDto dto)
        {
            await AttachBearerAsync();

            var body = new
            {
                anio = dto.Anno,
                numero_Periodo = dto.Numero,
                fecha_Inicio = dto.Fecha_Inicio,
                fecha_Fin = dto.Fecha_Fin
            };

            using var resp = await _http.PostAsJsonAsync("/periodo", body);

            if (!resp.IsSuccessStatusCode)
            {
                var detail = await resp.Content.ReadAsStringAsync();
                throw new Exception($"POST /periodo -> {(int)resp.StatusCode} {resp.StatusCode}. {detail}");
            }

            return true;
        }

        public async Task<bool> UpdateAsync(Models.PeriodoUpsertDto dto)
        {
            await AttachBearerAsync();

            var body = new
            {
                anio = dto.Anno,
                numero_Periodo = dto.Numero,
                fecha_Inicio = dto.Fecha_Inicio,
                fecha_Fin = dto.Fecha_Fin
            };

            using var resp = await _http.PutAsJsonAsync($"/periodo/{dto.ID_Periodo}", body);

            if (!resp.IsSuccessStatusCode)
            {
                var detail = await resp.Content.ReadAsStringAsync();
                throw new Exception(
                    $"PUT /periodo/{dto.ID_Periodo} -> {(int)resp.StatusCode} {resp.StatusCode}. {detail}"
                );
            }

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await AttachBearerAsync();

            using var resp = await _http.DeleteAsync($"/periodo/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}
