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

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // 
        // GET ALL
        // 
        public async Task<List<InstitucionDto>> GetAllAsync(string? q = null)
        {
            AddAuthorizationHeader();
            string url = "institucion";

            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error API Institución GetAll: {await res.Content.ReadAsStringAsync()}");
                return new List<InstitucionDto>();
            }

            var lista = await res.Content.ReadFromJsonAsync<List<InstitucionDto>>()
                        ?? new List<InstitucionDto>();

            // ✅ Filtrar en memoria si hay búsqueda
            if (!string.IsNullOrWhiteSpace(q))
            {
                lista = lista.Where(i => i.Nombre != null &&
                                         i.Nombre.Contains(q, StringComparison.OrdinalIgnoreCase))
                             .ToList();
            }

            return lista;
        }

        // 
        // GET BY ID
        // 
        public async Task<InstitucionDto?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();

            var res = await _http.GetAsync($"institucion/{id}");

            if (!res.IsSuccessStatusCode)
                return null;

            return await res.Content.ReadFromJsonAsync<InstitucionDto>();
        }

        // 
        // CREATE
        // 
        public async Task<(bool Ok, string Msg)> CreateAsync(InstitucionDto institucion)
        {
            AddAuthorizationHeader();

            var res = await _http.PostAsJsonAsync(
                $"institucion",
                institucion);

            return res.IsSuccessStatusCode
                ? (true, "Institución creada correctamente.")
                : (false, await res.Content.ReadAsStringAsync());
        }

        // 
        // UPDATE
        //
        public async Task<(bool Ok, string Msg)> UpdateAsync(int id, InstitucionDto institucion)
        {
            AddAuthorizationHeader();

            var res = await _http.PutAsJsonAsync(
                $"institucion/{id}",
                institucion);

            return res.IsSuccessStatusCode
                ? (true, "Institución actualizada correctamente.")
                : (false, await res.Content.ReadAsStringAsync());
        }

        //
        // DELETE
        //
        public async Task<(bool Ok, string Msg)> DeleteAsync(int id)
        {
            AddAuthorizationHeader();

            var res = await _http.DeleteAsync($"institucion/{id}");

            if (res.StatusCode == System.Net.HttpStatusCode.Conflict)
                return (false, "No se puede eliminar la institución, tiene carreras asociadas.");

            return res.IsSuccessStatusCode
                ? (true, "Institución eliminada correctamente.")
                : (false, await res.Content.ReadAsStringAsync());
        }
    }
}
