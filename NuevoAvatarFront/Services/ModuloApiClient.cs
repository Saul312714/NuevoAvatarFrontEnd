using System.Net.Http.Json;
using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public interface IModuloApiClient
    {
        Task<List<ModuloDto>> GetByRolAsync(int idRol);
    }

    public class ModuloApiClient : IModuloApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _contextAccessor;

        public ModuloApiClient(HttpClient http, IHttpContextAccessor contextAccessor)
        {
            _http = http;
            _contextAccessor = contextAccessor;
        }

        public async Task<List<ModuloDto>> GetByRolAsync(int idRol)
        {
            var token = _contextAccessor.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _http.GetAsync($"modulo/rol/{idRol}");
            if (!response.IsSuccessStatusCode)
                return new List<ModuloDto>();

            return await response.Content.ReadFromJsonAsync<List<ModuloDto>>() ?? new List<ModuloDto>();
        }
    }
}
