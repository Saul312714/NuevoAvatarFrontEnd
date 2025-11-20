using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public class ModuloApiClient : IModuloApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _context;

        public ModuloApiClient(HttpClient http, IHttpContextAccessor context)
        {
            _http = http;
            _context = context;
        }

        private void AddAuth()
        {
            var token = _context.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<ModuloDto>> GetAllAsync()
        {
            AddAuth();

            return await _http.GetFromJsonAsync<List<ModuloDto>>("/modulo?idUsuarioAccion=1")
                   ?? new List<ModuloDto>();
        }
    }
}
