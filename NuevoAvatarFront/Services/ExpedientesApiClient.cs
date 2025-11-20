using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public class ExpedienteApiClient : IExpedienteApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public ExpedienteApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        private void AddToken(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<ExpedienteDto>> GetAllAsync(int idUsuario, string token)
        {
            AddToken(token);

            var url = $"{_config["ApiMatricula"]}/expediente?idUsuario={idUsuario}";

            try
            {
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new List<ExpedienteDto>();

                return await response.Content.ReadFromJsonAsync<List<ExpedienteDto>>()
                       ?? new List<ExpedienteDto>();
            }
            catch
            {
                return new List<ExpedienteDto>();
            }
        }

        public async Task<ExpedienteDto?> GetByIdAsync(int id, int idUsuario, string token)
        {
            AddToken(token);

            var url = $"{_config["ApiMatricula"]}/expediente/{id}?idUsuario={idUsuario}";
            return await _http.GetFromJsonAsync<ExpedienteDto>(url);
        }

        public async Task<(bool ok, string? error)> CreateAsync(ExpedienteDto dto, int idUsuario, string token)
        {
            AddToken(token);

            var response = await _http.PostAsJsonAsync(
                $"{_config["ApiMatricula"]}/expediente?idUsuario={idUsuario}",
                dto
            );

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, await response.Content.ReadAsStringAsync());
        }

        public async Task<(bool ok, string? error)> UpdateAsync(ExpedienteDto dto, int idUsuario, string token)
        {
            AddToken(token);

            var response = await _http.PutAsJsonAsync(
                $"{_config["ApiMatricula"]}/expediente/{dto.ID_Estudiante}?idUsuario={idUsuario}",
                dto
            );

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, await response.Content.ReadAsStringAsync());
        }

        public async Task<(bool ok, string? error)> DeleteAsync(int id, int idUsuario, string token)
        {
            AddToken(token);

            var response = await _http.DeleteAsync(
                $"{_config["ApiMatricula"]}/expediente/{id}?idUsuario={idUsuario}");

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, await response.Content.ReadAsStringAsync());
        }
    }
}
