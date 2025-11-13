using System.Net.Http.Headers;
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

        //  SIEMPRE actualizar el token antes de cada request
        private void AddToken()
        {
            var token = _context.HttpContext?.Session.GetString("AccessToken");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        //  Obtener siempre el usuario en sesión
        private int GetUsuarioAccion()
        {
            var id = _context.HttpContext?.Session.GetInt32("UsuarioID") ?? 0;

            if (id == 0)
                Console.WriteLine("⚠ Advertencia: UsuarioID = 0 en CursoApiClient (revisar sesión/login)");

            return id;
        }


        public async Task<List<CursoDto>> GetAllAsync()
        {
            AddToken(); // << OBLIGATORIO
            int usuario = GetUsuarioAccion();

            var resp = await _http.GetAsync($"/curso?idUsuarioAccion={usuario}");

            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ Error en GetAllAsync: " + await resp.Content.ReadAsStringAsync());
                return new List<CursoDto>();
            }

            return await resp.Content.ReadFromJsonAsync<List<CursoDto>>() ?? new List<CursoDto>();
        }


        //  Obtener cursos por carrera y nivel
        public async Task<List<CursoDto>> GetByCarreraAndNivelAsync(int ID_Carrera, int? nivel)
        {
            AddToken();
            int idUsuarioAccion = GetUsuarioAccion();

            var url = $"curso?carreraId={ID_Carrera}&idUsuarioAccion={idUsuarioAccion}";

            if (nivel.HasValue)
                url += $"&nivel={nivel.Value}";

            var resp = await _http.GetAsync(url);

            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error GetByCarreraAndNivel: {await resp.Content.ReadAsStringAsync()}");
                return new List<CursoDto>();
            }

            return await resp.Content.ReadFromJsonAsync<List<CursoDto>>() ?? new List<CursoDto>();
        }

        //  Obtener curso por ID
        public async Task<CursoDto?> GetByIdAsync(int id)
        {
            AddToken();
            int idUsuarioAccion = GetUsuarioAccion();

            var resp = await _http.GetAsync($"curso/{id}?idUsuarioAccion={idUsuarioAccion}");

            if (!resp.IsSuccessStatusCode)
                return null;

            return await resp.Content.ReadFromJsonAsync<CursoDto>();
        }

        //  Crear curso
        public async Task<bool> CreateAsync(CursoCreateUpdateDto dto)
        {
            AddToken();
            int idUsuarioAccion = GetUsuarioAccion();

            var resp = await _http.PostAsJsonAsync(
                $"curso?idUsuarioAccion={idUsuarioAccion}", dto);

            if (!resp.IsSuccessStatusCode)
                Console.WriteLine($"❌ Error Create Curso: {await resp.Content.ReadAsStringAsync()}");

            return resp.IsSuccessStatusCode;
        }

        //  Actualizar curso
        public async Task<bool> UpdateAsync(int id, CursoCreateUpdateDto dto)
        {
            AddToken();
            int idUsuarioAccion = GetUsuarioAccion();

            var resp = await _http.PutAsJsonAsync(
                $"curso/{id}?idUsuarioAccion={idUsuarioAccion}", dto);

            if (!resp.IsSuccessStatusCode)
                Console.WriteLine($"❌ Error Update Curso: {await resp.Content.ReadAsStringAsync()}");

            return resp.IsSuccessStatusCode;
        }

        //  Eliminar curso
        public async Task<bool> DeleteAsync(int id)
        {
            AddToken();
            int idUsuarioAccion = GetUsuarioAccion();

            var resp = await _http.DeleteAsync(
                $"curso/{id}?idUsuarioAccion={idUsuarioAccion}");

            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error Delete Curso: {await resp.Content.ReadAsStringAsync()}");
            }

            return resp.IsSuccessStatusCode;
        }
    }
}
