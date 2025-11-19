using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using NuevoAvatarFront.Entities;
using System.Net.Http.Headers;

namespace NuevoAvatarFront.Services
{
    public class RolModuloApiClient : IRolModuloApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _context;

        public RolModuloApiClient(HttpClient http, IHttpContextAccessor context)
        {
            _http = http;
            _context = context;
        }

        // ============================
        // AGREGA EL TOKEN CORRECTAMENTE
        // ============================
        private void AddAuth()
        {
            var token = _context.HttpContext?.Session.GetString("AccessToken");

            if (string.IsNullOrWhiteSpace(token))
                return;

            // Limpiar duplicado
            if (_http.DefaultRequestHeaders.Contains("Authorization"))
                _http.DefaultRequestHeaders.Remove("Authorization");

            // Agregar
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }


        // ============================
        // GET permisos por rol
        // ============================
        public async Task<List<RolModuloDto>> GetByRolAsync(int idRol)
        {
            AddAuth();

            var request = new HttpRequestMessage(HttpMethod.Get, $"/rolmodulo/{idRol}");
            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return new List<RolModuloDto>();  // evita 401

            var data = await response.Content.ReadFromJsonAsync<List<RolModuloDto>>();
            return data ?? new List<RolModuloDto>();
        }


        // ============================
        // UPDATE permisos por rol
        // ============================
        public async Task<bool> UpdatePermisosAsync(int idRol, List<RolModuloDto> permisos)
        {
            AddAuth();

            var res = await _http.PutAsJsonAsync($"/rolmodulo/{idRol}", permisos);
            return res.IsSuccessStatusCode;
        }
    }
}
