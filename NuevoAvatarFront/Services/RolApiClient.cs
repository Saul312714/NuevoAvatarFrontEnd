using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public class RolApiClient : IRolApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _context;

        public RolApiClient(HttpClient httpClient, IHttpContextAccessor context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        // Método para agregar el token de autorización en los encabezados
        private void AddAuthHeader()
        {
            var token = _context.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // Obtener todos los roles
        public async Task<List<RolDto>> GetAllRolesAsync(int idUsuarioAccion)
        {
            AddAuthHeader(); // Asegúrate de que el token se agregue a la solicitud

            var response = await _httpClient.GetFromJsonAsync<List<RolDto>>($"/rol?idUsuarioAccion={idUsuarioAccion}");
            return response ?? new List<RolDto>();
        }

        // Obtener rol por ID
        public async Task<RolDto?> GetByIdAsync(int id, int idUsuarioAccion)
        {
            AddAuthHeader(); // Asegúrate de que el token se agregue a la solicitud

            var response = await _httpClient.GetFromJsonAsync<RolDto>($"/rol/{id}?idUsuarioAccion={idUsuarioAccion}");
            return response;
        }

        // Crear rol
        public async Task CreateAsync(RolDto rol, int idUsuarioAccion)
        {
            AddAuthHeader(); // Asegúrate de que el token se agregue a la solicitud

            await _httpClient.PostAsJsonAsync($"/rol?idUsuarioAccion={idUsuarioAccion}", rol);
        }

        // Actualizar rol
        public async Task UpdateAsync(RolDto rol, int idUsuarioAccion)
        {
            AddAuthHeader(); // Asegúrate de que el token se agregue a la solicitud

            await _httpClient.PutAsJsonAsync($"/rol/{rol.ID_Rol}?idUsuarioAccion={idUsuarioAccion}", rol);
        }

        // Eliminar rol
        public async Task<bool> DeleteAsync(int id, int idUsuarioAccion)
        {
            AddAuthHeader(); // Asegúrate de que el token se agregue a la solicitud

            var response = await _httpClient.DeleteAsync($"/rol/{id}?idUsuarioAccion={idUsuarioAccion}");
            return response.IsSuccessStatusCode;
        }
    }
}
