using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;
using System.Net.Http.Json;

namespace NuevoAvatarFront.Services
{
    public interface IEstudianteService
    {
        Task<List<EstudianteDto>> GetAllAsync();
        Task<List<EstudianteDto>> GetByCursoYGrupoAsync(int idCurso, int idGrupo);
    }

    public class EstudianteService : ApiClientBase, IEstudianteService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EstudianteService> _logger;

        public EstudianteService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            TokenHelper tokenHelper,
            ILogger<EstudianteService> logger
        ) : base(httpContextAccessor, tokenHelper)
        {
            _httpClient = httpClientFactory.CreateClient("MatriculaClient");
            _logger = logger;
        }

        public async Task<List<EstudianteDto>> GetAllAsync()
        {
            try
            {
                await AttachBearerAsync(_httpClient);

                var idUsuario = _httpContextAccessor.HttpContext?
                    .Session.GetInt32("UsuarioID") ?? 0;

                if (idUsuario == 0)
                {
                    _logger.LogWarning(" No se encontró UsuarioID en sesión. Devolviendo lista vacía.");
                    return new List<EstudianteDto>();
                }

                var response = await _httpClient.GetAsync($"/expediente?idUsuario={idUsuario}");

                await EnsureSuccessOrThrowAsync(
                    response,
                    _logger,
                    "No fue posible obtener el listado de estudiantes."
                );

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<EstudianteDto>>();
                    return data ?? new List<EstudianteDto>();
                }

                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Error al obtener estudiantes. Status: {Status}, Error: {Error}",
                    response.StatusCode, err
                );

                return new List<EstudianteDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estudiantes");
                throw;
            }
        }

        /// Obtiene los estudiantes matriculados en un curso y grupo específicos
        public async Task<List<EstudianteDto>> GetByCursoYGrupoAsync(int idCurso, int idGrupo)
        {
            try
            {
                await AttachBearerAsync(_httpClient);

                var idUsuario = _httpContextAccessor.HttpContext?
                    .Session.GetInt32("UsuarioID") ?? 0;

                if (idUsuario == 0)
                {
                    _logger.LogWarning(" No se encontró UsuarioID en sesión. Devolviendo lista vacía.");
                    return new List<EstudianteDto>();
                }

                var url = $"/matricula/curso/{idCurso}/grupo/{idGrupo}?idUsuario={idUsuario}";
                var response = await _httpClient.GetAsync(url);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // No hay estudiantes matriculados en ese curso y grupo
                    return new List<EstudianteDto>();
                }

                await EnsureSuccessOrThrowAsync(
                    response,
                    _logger,
                    "No fue posible obtener los estudiantes del curso y grupo."
                );

                var data = await response.Content.ReadFromJsonAsync<List<EstudianteDto>>();
                return data ?? new List<EstudianteDto>();
            }
            catch (ApiException)
            {
                // Las ApiException ya vienen con mensaje controlado
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error no controlado al obtener estudiantes por curso {Curso} y grupo {Grupo}",
                    idCurso, idGrupo);
                throw;
            }
        }
    }
}
