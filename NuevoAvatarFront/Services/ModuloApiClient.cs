using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public class ModuloApiClient : IModuloApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _context;
        private const string BaseEndpoint = "/modulo";

        public ModuloApiClient(HttpClient http, IHttpContextAccessor context)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Agrega el token al header (limpiando tokens viejos)
        private void AddAuth()
        {
            var token = _context.HttpContext?.Session.GetString("AccessToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                // limpiar si ya existe
                if (_http.DefaultRequestHeaders.Contains("Authorization"))
                    _http.DefaultRequestHeaders.Remove("Authorization");

                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        // Obtiene ID del usuario desde la sesión (correcto: UsuarioID)
        private int GetIdUsuarioAccion()
        {
            var idUsuario = _context.HttpContext?.Session.GetInt32("UsuarioID");

            // Si NO hay sesión, devolvemos 1 para evitar crash
            return idUsuario ?? 1;
        }

        // GET TODOS LOS MÓDULOS
        public async Task<List<ModuloDto>> GetAllAsync()
        {
            AddAuth();
            var idUsuarioAccion = GetIdUsuarioAccion();

            try
            {
                var result = await _http.GetFromJsonAsync<List<ModuloDto>>(
                    $"{BaseEndpoint}?idUsuarioAccion={idUsuarioAccion}"
                );

                return result ?? new List<ModuloDto>();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener módulos", ex);
            }
        }

        // GET POR ID
        public async Task<ModuloDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a 0", nameof(id));

            AddAuth();
            var idUsuarioAccion = GetIdUsuarioAccion();

            try
            {
                return await _http.GetFromJsonAsync<ModuloDto>(
                    $"{BaseEndpoint}/{id}?idUsuarioAccion={idUsuarioAccion}"
                );
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al obtener el módulo con ID {id}", ex);
            }
        }

        // CREAR MÓDULO
        public async Task<ModuloDto?> CreateAsync(ModuloDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            AddAuth();
            var idUsuarioAccion = GetIdUsuarioAccion();

            try
            {
                var response = await _http.PostAsJsonAsync(
                    $"{BaseEndpoint}?idUsuarioAccion={idUsuarioAccion}",
                    dto
                );

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ModuloDto>();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al crear módulo", ex);
            }
        }

        // ACTUALIZAR MÓDULO
        public async Task<ModuloDto?> UpdateAsync(ModuloDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.ID_Modulo <= 0)
                throw new ArgumentException("El ID del módulo debe ser válido", nameof(dto));

            AddAuth();
            var idUsuarioAccion = GetIdUsuarioAccion();

            try
            {
                var response = await _http.PutAsJsonAsync(
                    $"{BaseEndpoint}/{dto.ID_Modulo}?idUsuarioAccion={idUsuarioAccion}",
                    dto
                );

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ModuloDto>();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al actualizar módulo {dto.ID_Modulo}", ex);
            }
        }

        public async Task<(bool ok, string? errorMessage)> DeleteAsync(int id)
        {
            AddAuth();
            var idUsuarioAccion = GetIdUsuarioAccion();

            try
            {
                var response = await _http.DeleteAsync(
                    $"{BaseEndpoint}/{id}?idUsuarioAccion={idUsuarioAccion}"
                );

                if (response.IsSuccessStatusCode)
                    return (true, null);

                // Leer mensaje de error del backend
                var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                string? errorMessage = errorObj != null && errorObj.ContainsKey("error")
                    ? errorObj["error"]
                    : "No se pudo eliminar el módulo.";

                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                return (false, $"Excepción: {ex.Message}");
            }
        }

    }
}
