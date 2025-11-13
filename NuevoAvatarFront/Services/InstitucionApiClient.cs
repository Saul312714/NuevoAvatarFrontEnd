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

        //  Método privado para agregar el token a cada solicitud
        private void AddAuthorizationHeader()
        {
            var token = _contextAccessor.HttpContext?.Session.GetString("AccessToken");

            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        //  Obtener todas las instituciones (con filtro opcional)
        public async Task<List<InstitucionDto>> GetAllAsync(string? q = null)
        {
            AddAuthorizationHeader();

            var url = string.IsNullOrWhiteSpace(q)
                ? "institucion"
                : $"institucion?q={Uri.EscapeDataString(q)}";  // Filtrar por nombre

            var data = await _http.GetFromJsonAsync<List<InstitucionDto>>(url);
            return data ?? new List<InstitucionDto>();
        }


        //  Obtener una institución por ID
        public async Task<InstitucionDto?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();

            var response = await _http.GetAsync($"institucion/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<InstitucionDto>();
        }

        //  Crear una nueva institución
        public async Task<(bool Ok, string Msg)> CreateAsync(InstitucionDto institucion)
        {
            AddAuthorizationHeader();

            var resp = await _http.PostAsJsonAsync("institucion", institucion);

            return resp.IsSuccessStatusCode
                ? (true, "Institución creada correctamente.")
                : (false, await resp.Content.ReadAsStringAsync());
        }

        //  Actualizar institución existente
        public async Task<(bool Ok, string Msg)> UpdateAsync(int id, InstitucionDto institucion)
        {
            AddAuthorizationHeader();

            var resp = await _http.PutAsJsonAsync($"institucion/{id}", institucion);

            return resp.IsSuccessStatusCode
                ? (true, "Institución actualizada correctamente.")
                : (false, await resp.Content.ReadAsStringAsync());
        }

        //  Eliminar institución
        public async Task<(bool Ok, string Msg)> DeleteAsync(int id)
        {
            AddAuthorizationHeader();

            var resp = await _http.DeleteAsync($"institucion/{id}");

            if (resp.StatusCode == System.Net.HttpStatusCode.Conflict)
                return (false, "No se puede eliminar la institución, tiene carreras asociadas.");

            return resp.IsSuccessStatusCode
                ? (true, "Institución eliminada correctamente.")
                : (false, await resp.Content.ReadAsStringAsync());
        }
    }
}
