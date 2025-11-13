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

            // SIEMPRE sobrescribir, aunque antes estuviera vacío
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private int GetUsuarioAccion()
        {
            var id = _contextAccessor.HttpContext?.Session.GetInt32("UsuarioID") ?? 0;

            if (id == 0)
                Console.WriteLine(" ADVERTENCIA: UsuarioID viene en 0. Revisar login o sesión.");

            return id;
        }

        // Obtener todas
        public async Task<List<InstitucionDto>> GetAllAsync(string? q = null)
        {
            AddAuthorizationHeader();
            int idUsuarioAccion = GetUsuarioAccion();

            string url = $"institucion?idUsuarioAccion={idUsuarioAccion}";
            if (!string.IsNullOrWhiteSpace(q))
                url = $"institucion?q={Uri.EscapeDataString(q)}&idUsuarioAccion={idUsuarioAccion}";

            var res = await _http.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error API Institución GetAll: {await res.Content.ReadAsStringAsync()}");
                return new List<InstitucionDto>();
            }

            return await res.Content.ReadFromJsonAsync<List<InstitucionDto>>() ?? new List<InstitucionDto>();
        }

        // Obtener por ID
        public async Task<InstitucionDto?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();
            int idUsuarioAccion = GetUsuarioAccion();

            var res = await _http.GetAsync($"institucion/{id}?idUsuarioAccion={idUsuarioAccion}");

            if (!res.IsSuccessStatusCode)
                return null;

            return await res.Content.ReadFromJsonAsync<InstitucionDto>();
        }

        // Crear
        public async Task<(bool Ok, string Msg)> CreateAsync(InstitucionDto institucion)
        {
            AddAuthorizationHeader();
            int idUsuarioAccion = GetUsuarioAccion();

            var res = await _http.PostAsJsonAsync(
                $"institucion?idUsuarioAccion={idUsuarioAccion}",
                institucion);

            return res.IsSuccessStatusCode
                ? (true, "Institución creada correctamente.")
                : (false, await res.Content.ReadAsStringAsync());
        }

        // Actualizar
        public async Task<(bool Ok, string Msg)> UpdateAsync(int id, InstitucionDto institucion)
        {
            AddAuthorizationHeader();
            int idUsuarioAccion = GetUsuarioAccion();

            var res = await _http.PutAsJsonAsync(
                $"institucion/{id}?idUsuarioAccion={idUsuarioAccion}",
                institucion);

            return res.IsSuccessStatusCode
                ? (true, "Institución actualizada correctamente.")
                : (false, await res.Content.ReadAsStringAsync());
        }

        // Eliminar
        public async Task<(bool Ok, string Msg)> DeleteAsync(int id)
        {
            AddAuthorizationHeader();
            int idUsuarioAccion = GetUsuarioAccion();

            var res = await _http.DeleteAsync(
                $"institucion/{id}?idUsuarioAccion={idUsuarioAccion}");

            if (res.StatusCode == System.Net.HttpStatusCode.Conflict)
                return (false, "No se puede eliminar la institución, tiene carreras asociadas.");

            return res.IsSuccessStatusCode
                ? (true, "Institución eliminada correctamente.")
                : (false, await res.Content.ReadAsStringAsync());
        }
    }
}
