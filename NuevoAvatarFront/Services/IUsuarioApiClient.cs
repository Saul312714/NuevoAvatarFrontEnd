using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public interface IUsuarioApiClient
    {
        Task<List<UsuarioDto>> GetAllAsync(string? filtro = null, string? rol = null, int page = 1, int pageSize = 10);
        Task<UsuarioDto?> GetByIdAsync(int id, int idUsuarioAccion);
        Task<UsuarioDto?> CreateAsync(UsuarioDto usuario, int idUsuarioAccion);
        Task<UsuarioDto?> UpdateAsync(UsuarioDto usuario, int idUsuarioAccion);
        Task<bool> DeleteAsync(int id, int idUsuarioAccion);
    }

    public class UsuarioApiClient : IUsuarioApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _context;

        public UsuarioApiClient(HttpClient http, IHttpContextAccessor context)
        {
            _http = http;
            _context = context;
        }

        private void AddAuthHeader()
        {
            var token = _context.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Obtener todos
        public async Task<List<UsuarioDto>> GetAllAsync(string? filtro = null, string? rol = null, int page = 1, int pageSize = 10)
        {
            AddAuthHeader();

            // Obtener el ID del usuario en sesión (idUsuarioAccion)
            var idUsuarioAccion = _context.HttpContext?.Session.GetInt32("UsuarioID") ?? 0;

            // Construir la URL con los parámetros de paginación y filtros
            var url = $"usuario?idUsuarioAccion={idUsuarioAccion}&page={page}&pageSize={pageSize}";

            if (!string.IsNullOrEmpty(filtro))
                url += $"&filtro={filtro}";
            if (!string.IsNullOrEmpty(rol))
                url += $"&rol={rol}";

            // Realizar la solicitud GET
            var res = await _http.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                var msg = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"Error API GetAllUsuarios: {msg}");
                return new List<UsuarioDto>(); // Retorna una lista vacía si hay un error
            }

            return await res.Content.ReadFromJsonAsync<List<UsuarioDto>>() ?? new List<UsuarioDto>(); // Devuelve los resultados obtenidos
        }


        // Obtener por ID
        public async Task<UsuarioDto?> GetByIdAsync(int id, int idUsuarioAccion)
        {
            AddAuthHeader();
            var url = $"usuario/{id}?idUsuarioAccion={idUsuarioAccion}";
            var res = await _http.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"Error GetById: {error}");
                return null;
            }

            return await res.Content.ReadFromJsonAsync<UsuarioDto>();
        }

        // Crear usuario
        public async Task<UsuarioDto?> CreateAsync(UsuarioDto usuario, int idUsuarioAccion)
        {
            AddAuthHeader();
            var url = $"usuario?idUsuarioAccion={idUsuarioAccion}";

            var payload = new
            {
                email = usuario.Email,
                tipo_Identificacion = usuario.Tipo_Identificacion,
                identificacion = usuario.Identificacion,
                nombre = usuario.Nombre,
                contrasena = usuario.Contrasena,
                iD_Rol = usuario.ID_Rol,
                rolNombre = usuario.RolNombre
            };

            var res = await _http.PostAsJsonAsync(url, payload);

            if (!res.IsSuccessStatusCode)
            {
                var msg = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"Error API CrearUsuario: {msg}");
                return null;
            }

            return await res.Content.ReadFromJsonAsync<UsuarioDto>();
        }

        // Actualizar usuario
        public async Task<UsuarioDto?> UpdateAsync(UsuarioDto usuario, int idUsuarioAccion)
        {
            AddAuthHeader();
            var url = $"usuario/{usuario.ID_Usuario}?idUsuarioAccion={idUsuarioAccion}";

            var payload = new
            {
                iD_Usuario = usuario.ID_Usuario,
                email = usuario.Email,
                tipo_Identificacion = usuario.Tipo_Identificacion,
                identificacion = usuario.Identificacion,
                nombre = usuario.Nombre,
                contrasena = usuario.Contrasena,
                iD_Rol = usuario.ID_Rol,
                rolNombre = usuario.RolNombre
            };

            var res = await _http.PutAsJsonAsync(url, payload);

            if (!res.IsSuccessStatusCode)
            {
                var msg = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"Error API UpdateUsuario: {msg}");
                return null;
            }

            return await res.Content.ReadFromJsonAsync<UsuarioDto>();
        }

        // Eliminar usuario
        public async Task<bool> DeleteAsync(int id, int idUsuarioAccion)
        {
            AddAuthHeader();
            var url = $"usuario/{id}?idUsuarioAccion={idUsuarioAccion}";
            var res = await _http.DeleteAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                var msg = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"Error API DeleteUsuario: {msg}");
            }

            return res.IsSuccessStatusCode;
        }
    }
}
