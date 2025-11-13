using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace NuevoAvatarFront.Services
{
    public class PeriodoClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpCtx;
        private readonly IConfiguration _cfg;

        public PeriodoClient(HttpClient http, IHttpContextAccessor httpCtx, IConfiguration cfg)
        {
            _http = http;
            _httpCtx = httpCtx;
            _cfg = cfg;
        }

        private void AttachBearer()
        {
            var token = _httpCtx.HttpContext?.Session.GetString("JWT");

            // Si no hay token en sesión, usa el token de appsettings.json
            if (string.IsNullOrWhiteSpace(token))
                token = _cfg["Auth:DevBearerToken"];

            _http.DefaultRequestHeaders.Authorization =
                string.IsNullOrWhiteSpace(token)
                    ? null
                    : new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<Models.PeriodoDto>> GetAllAsync()
        {
            AttachBearer();
            using var resp = await _http.GetAsync("/periodo");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<Models.PeriodoDto>>() ?? new();
        }

        public async Task<Models.PeriodoDto?> GetByIdAsync(int id)
        {
            AttachBearer();
            using var resp = await _http.GetAsync($"/periodo/{id}");
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<Models.PeriodoDto?>();
        }

        public async Task<bool> CreateAsync(Models.PeriodoUpsertDto dto)
        {
            AttachBearer();
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
            AttachBearer();
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
                throw new Exception($"PUT /periodo/{dto.ID_Periodo} -> {(int)resp.StatusCode} {resp.StatusCode}. {detail}");
            }
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            AttachBearer();
            using var resp = await _http.DeleteAsync($"/periodo/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}