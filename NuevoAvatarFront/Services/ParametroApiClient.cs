using NuevoAvatarFront.Entities;
using System.Net.Http.Headers;

namespace NuevoAvatarFront.Services
{
    public class ParametroApiClient : IParametroApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public ParametroApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        private void AddToken(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<ParametroDto>> GetAllAsync(int idUsuario, string token)
        {
            AddToken(token);

            var url = $"{_config["ApiUsuariosRoles"]}/parametro?idUsuarioAccion={idUsuario}";
            return await _http.GetFromJsonAsync<List<ParametroDto>>(url) ?? new();
        }

        public async Task<ParametroDto?> GetByIdAsync(string id, int idUsuario, string token)
        {
            AddToken(token);

            var url = $"{_config["ApiUsuariosRoles"]}/parametro/{id}?idUsuarioAccion={idUsuario}";
            return await _http.GetFromJsonAsync<ParametroDto>(url);
        }

        public async Task<(bool ok, string? error)> CreateAsync(ParametroDto dto, int idUsuario, string token)
        {
            AddToken(token);

            var response = await _http.PostAsJsonAsync(
                $"{_config["ApiUsuariosRoles"]}/parametro?idUsuarioAccion={idUsuario}",
                dto);

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, await response.Content.ReadAsStringAsync());
        }

        public async Task<(bool ok, string? error)> UpdateAsync(ParametroDto dto, int idUsuario, string token)
        {
            AddToken(token);

            var response = await _http.PutAsJsonAsync(
                $"{_config["ApiUsuariosRoles"]}/parametro/{dto.ID_Parametro}?idUsuarioAccion={idUsuario}",
                dto);

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, await response.Content.ReadAsStringAsync());
        }

        public async Task<(bool ok, string? error)> DeleteAsync(string id, int idUsuario, string token)
        {
            AddToken(token);

            var response = await _http.DeleteAsync(
                $"{_config["ApiUsuariosRoles"]}/parametro/{id}?idUsuarioAccion={idUsuario}");

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, await response.Content.ReadAsStringAsync());
        }
    }
}
