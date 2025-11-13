using Microsoft.AspNetCore.Http;
using NuevoAvatarFront.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NuevoAvatarFront.Services
{
    public class InstitucionApiClient : IInstitucionApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _contextAccessor;

        public InstitucionApiClient(HttpClient http, IHttpContextAccessor contextAccessor)
        {
            _http = http;
            _contextAccessor = contextAccessor;
        }

        private void AddAuthorizationHeader()
        {
            var token = _contextAccessor.HttpContext?.Session.GetString("AccessToken");

            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private int GetUsuarioAccion()
        {
            return _contextAccessor.HttpContext?.Session.GetInt32("UsuarioID") ?? 0;
        }

        // Obtener todas
        public async Task<List<InstitucionDto>> GetAllAsync(string? q = null)
        {
            AddAuthorizationHeader();

            int idUsuarioAccion = GetUsuarioAccion();

            var url = string.IsNullOrWhiteSpace(q)
                ? $"institucion?idUsuarioAccion={idUsuarioAccion}"
                : $"institucion?q={Uri.EscapeDataString(q)}&idUsuarioAccion={idUsuarioAccion}";

            var data = await _http.GetFromJsonAsync<List<InstitucionDto>>(url);
            return data ?? new List<InstitucionDto>();
        }

        // Obtener por ID
        public async Task<InstitucionDto?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();

            int idUsuarioAccion = GetUsuarioAccion();

            var response = await _http.GetAsync($"institucion/{id}?idUsuarioAccion={idUsuarioAccion}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<InstitucionDto>();
        }

        // Crear
        public async Task<(bool Ok, string Msg)> CreateAsync(InstitucionDto institucion)
        {
            AddAuthorizationHeader();

            int idUsuarioAccion = GetUsuarioAccion();

            var resp = await _http.PostAsJsonAsync(
                $"institucion?idUsuarioAccion={idUsuarioAccion}",
                institucion);

            return resp.IsSuccessStatusCode
                ? (true, "Institución creada correctamente.")
                : (false, await resp.Content.ReadAsStringAsync());
        }

        // Actualizar
        public async Task<(bool Ok, string Msg)> UpdateAsync(int id, InstitucionDto institucion)
        {
            AddAuthorizationHeader();

            int idUsuarioAccion = GetUsuarioAccion();

            var resp = await _http.PutAsJsonAsync(
                $"institucion/{id}?idUsuarioAccion={idUsuarioAccion}",
                institucion);

            return resp.IsSuccessStatusCode
                ? (true, "Institución actualizada correctamente.")
                : (false, await resp.Content.ReadAsStringAsync());
        }

        // Eliminar
        public async Task<(bool Ok, string Msg)> DeleteAsync(int id)
        {
            AddAuthorizationHeader();

            int idUsuarioAccion = GetUsuarioAccion();

            var resp = await _http.DeleteAsync(
                $"institucion/{id}?idUsuarioAccion={idUsuarioAccion}");

            if (resp.StatusCode == System.Net.HttpStatusCode.Conflict)
                return (false, "No se puede eliminar la institución, tiene carreras asociadas.");

            return resp.IsSuccessStatusCode
                ? (true, "Institución eliminada correctamente.")
                : (false, await resp.Content.ReadAsStringAsync());
        }
    }
}
