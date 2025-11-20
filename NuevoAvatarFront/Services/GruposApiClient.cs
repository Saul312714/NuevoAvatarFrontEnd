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

        private string BaseUrl =>
            _config["OfertaAcademicaApi:BaseUrl"]
                ?? throw new InvalidOperationException("OfertaAcademicaApi:BaseUrl no configurado");

        private void AddToken(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }



        public async Task<List<GrupoDto>> GetAllAsync(string token)
        {
            AddToken(token);

            return await _http.GetFromJsonAsync<List<GrupoDto>>($"{BaseUrl}/grupo")
                ?? new List<GrupoDto>();
        }



        public async Task<GrupoDto?> GetByIdAsync(int id, int userId, string token)
        {
            AddToken(token);

            return await _http.GetFromJsonAsync<GrupoDto>(
                $"{BaseUrl}/grupo/{id}?idUsuarioAccion={userId}");
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
                ID_Periodo = dto.ID_Periodo
            };

            var response = await _http.PostAsJsonAsync(
                $"{BaseUrl}/grupo?idUsuarioAccion={userId}",
                body
            );

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
                $"{BaseUrl}/grupo/{dto.ID_Grupo}?idUsuarioAccion={userId}",
                body
            );

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, await response.Content.ReadAsStringAsync());
        }



        public async Task<(bool ok, string? error)> DeleteAsync(int id, int userId, string token)
        {
            AddToken(token);

            var response = await _http.DeleteAsync(
                $"{BaseUrl}/grupo/{id}?idUsuarioAccion={userId}"
            );

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, await response.Content.ReadAsStringAsync());
        }


    
        public async Task<List<CursoDto>> GetCursosAsync(string token)
        {
            AddToken(token);

            return await _http.GetFromJsonAsync<List<CursoDto>>($"{BaseUrl}/curso")
                ?? new List<CursoDto>();
        }



        public async Task<List<ProfesorDto>> GetProfesoresAsync(string token)
        {
            AddToken(token);

            return await _http.GetFromJsonAsync<List<ProfesorDto>>($"{BaseUrl}/profesor")
                ?? new List<ProfesorDto>();
        }


    
        public async Task<List<PeriodoDto>> GetPeriodosAsync(int userId, string token)
        {
            AddToken(token);

            return await _http.GetFromJsonAsync<List<PeriodoDto>>(
                $"{BaseUrl}/periodo?idUsuarioAccion={userId}"
            ) ?? new List<PeriodoDto>();
        }
    }
}
