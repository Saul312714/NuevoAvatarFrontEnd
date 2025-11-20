using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public class GrupoApiClient : IGrupoApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public GrupoApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        private void AddToken(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }


        public async Task<List<GrupoDto>> GetAllAsync(string token)
        {
            AddToken(token);

            return await _http.GetFromJsonAsync<List<GrupoDto>>(
                $"{_config["ApiOfertaAcademica"]}/grupo")
                ?? new List<GrupoDto>();
        }

        public async Task<GrupoDto?> GetByIdAsync(int id, int userId, string token)
        {
            AddToken(token);

            return await _http.GetFromJsonAsync<GrupoDto>(
                $"{_config["ApiOfertaAcademica"]}/grupo/{id}?idUsuarioAccion={userId}");
        }

        public async Task<(bool ok, string? error)> CreateAsync(GrupoDto dto, int userId, string token)
        {
            AddToken(token);

            var body = new
            {
                Numero_Grupo = dto.Numero,
                ID_Curso = dto.ID_Curso,
                ID_Profesor = dto.ID_Profesor,
                Horario = dto.Horario,
                ID_Periodo = dto.ID_Periodo,
                idUsuarioAccion = userId
            };

            var response = await _http.PostAsJsonAsync(
                $"{_config["ApiOfertaAcademica"]}/grupo", body);

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, await response.Content.ReadAsStringAsync());
        }

        public async Task<(bool ok, string? error)> UpdateAsync(GrupoDto dto, int userId, string token)
        {
            AddToken(token);

            var body = new
            {
                Numero_Grupo = dto.Numero,
                ID_Curso = dto.ID_Curso,
                ID_Profesor = dto.ID_Profesor,
                Horario = dto.Horario,
                ID_Periodo = dto.ID_Periodo
            };

            var response = await _http.PutAsJsonAsync(
                $"{_config["ApiOfertaAcademica"]}/grupo/{dto.ID_Grupo}?idUsuarioAccion={userId}",
                body);

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, await response.Content.ReadAsStringAsync());
        }

        public async Task<(bool ok, string? error)> DeleteAsync(int id, int userId, string token)
        {
            AddToken(token);

            var response = await _http.DeleteAsync(
                $"{_config["ApiOfertaAcademica"]}/grupo/{id}?idUsuarioAccion={userId}");

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, await response.Content.ReadAsStringAsync());
        }


        public async Task<List<CursoDto>> GetCursosAsync(string token)
        {
            AddToken(token);
            return await _http.GetFromJsonAsync<List<CursoDto>>(
                $"{_config["ApiOfertaAcademica"]}/curso") ?? new();
        }

        public async Task<List<ProfesorDto>> GetProfesoresAsync(string token)
        {
            AddToken(token);
            return await _http.GetFromJsonAsync<List<ProfesorDto>>(
                $"{_config["ApiOfertaAcademica"]}/profesor") ?? new();
        }

        public async Task<List<PeriodoDto>> GetPeriodosAsync(int userId, string token)
        {
            AddToken(token);
            return await _http.GetFromJsonAsync<List<PeriodoDto>>(
                $"{_config["ApiOfertaAcademica"]}/periodo?idUsuarioAccion={userId}") ?? new();
        }
    }
}
