using Microsoft.AspNetCore.Http;
using NuevoAvatarFront.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NuevoAvatarFront.Services
{
    public class CursoApiClient : ICursoApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _contextAccessor;

        public CursoApiClient(HttpClient http, IHttpContextAccessor contextAccessor)
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
        public async Task<List<CursoDto>> GetAllAsync(int? nivel = null, int? idCarrera = null)
        {
            AddAuthorizationHeader();
            string url = "curso";

            var res = await _http.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error API Curso GetAll: {await res.Content.ReadAsStringAsync()}");
                return new List<CursoDto>();
            }

            var lista = await res.Content.ReadFromJsonAsync<List<CursoDto>>()
                        ?? new List<CursoDto>();

            // Filtro en memoria: nivel
            if (nivel is not null)
                lista = lista.Where(c => c.Nivel == nivel).ToList();

            // Filtro en memoria: carrera
            if (idCarrera is not null)
                lista = lista.Where(c => c.ID_Carrera == idCarrera).ToList();

            return lista;
        }

        //
        // GET x ID
        // 
        public async Task<CursoDto?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();

            var res = await _http.GetAsync($"curso/{id}");

            if (!res.IsSuccessStatusCode)
                return null;

            return await res.Content.ReadFromJsonAsync<CursoDto>();
        }

        // 
        // GET x CARRERA
        // 
        public async Task<List<CursoDto>> GetByCarreraAsync(int idCarrera)
        {
            AddAuthorizationHeader();

            var res = await _http.GetAsync($"curso/carrera/{idCarrera}");

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error API Curso GetByCarrera: {await res.Content.ReadAsStringAsync()}");
                return new List<CursoDto>();
            }

            return await res.Content.ReadFromJsonAsync<List<CursoDto>>()
                   ?? new List<CursoDto>();
        }

        // 
        // CREATE
        // 
        public async Task<(bool Ok, string Msg)> CreateAsync(CursoCreateDto dto)
        {
            AddAuthorizationHeader();

            var res = await _http.PostAsJsonAsync($"curso", dto);

            if (res.IsSuccessStatusCode)
                return (true, "Curso creado correctamente.");

            return (false, await res.Content.ReadAsStringAsync());
        }

        // 
        // UPDATE
        // 
        public async Task<(bool Ok, string Msg)> UpdateAsync(int id, CursoUpdateDto dto)
        {
            AddAuthorizationHeader();

            var res = await _http.PutAsJsonAsync($"curso/{id}", dto);

            return res.IsSuccessStatusCode
                ? (true, "Curso actualizado correctamente.")
                : (false, await res.Content.ReadAsStringAsync());
        }

        // 
        // DELETE
        // 
        public async Task<(bool Ok, string Msg)> DeleteAsync(int id)
        {
            AddAuthorizationHeader();

            var res = await _http.DeleteAsync($"curso/{id}");

            string content = await res.Content.ReadAsStringAsync();

            // backend manda algo como:  "Mensaje": "No se puede eliminar..." 
            if (!res.IsSuccessStatusCode)
            {
                return (false, content);
            }

            return (true, "Curso eliminado correctamente.");
        }
    }
}
