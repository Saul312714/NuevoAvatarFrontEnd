using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;

namespace NuevoAvatarFront.Services
{
    public interface ICursoService
    {
        Task<List<CursoDTO>> GetNivel1Async();
        Task<List<CursoDTO>> GetNivel1PorCarreraAsync(int idCarrera);
        Task<List<CursoDTO>> GetAllAsync();
    }

    public class CursoService : ApiClientBase, ICursoService
    {
        private readonly HttpClient _http;
        private readonly ILogger<CursoService> _logger;

        public CursoService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CursoService> logger)
            : base(httpContextAccessor)
        {
            _http = httpClientFactory.CreateClient("OfertaClient");
            _logger = logger;
        }

        public async Task<List<CursoDTO>> GetNivel1Async()
        {
            try
            {
                AttachBearer(_http);

                var response = await _http.GetAsync("/curso");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al obtener cursos. Status: {Status}, Error: {Error}",
                        response.StatusCode, error);

                    return new List<CursoDTO>();
                }

                var data = await response.Content.ReadFromJsonAsync<List<CursoDTO>>()
                           ?? new List<CursoDTO>();

                // Solo nivel 1
                return data
                    .Where(c => c.Nivel == 1)
                    .OrderBy(c => c.Nombre)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consumir la API de cursos");
                throw;
            }
        }

        public async Task<List<CursoDTO>> GetNivel1PorCarreraAsync(int idCarrera)
        {
            try
            {
                AttachBearer(_http);

                var response = await _http.GetAsync("/curso");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al obtener cursos por carrera. Status: {Status}, Error: {Error}",
                        response.StatusCode, error);

                    return new List<CursoDTO>();
                }

                var data = await response.Content.ReadFromJsonAsync<List<CursoDTO>>()
                           ?? new List<CursoDTO>();

                return data
                    .Where(c => c.Nivel == 1 && c.ID_Carrera == idCarrera)
                    .OrderBy(c => c.Nombre)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consumir la API de cursos por carrera");
                throw;
            }
        }

        public async Task<List<CursoDTO>> GetAllAsync()
        {
            try
            {
                AttachBearer(_http);

                var response = await _http.GetAsync("/curso");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al obtener todos los cursos. Status: {Status}, Error: {Error}",
                        response.StatusCode, error);

                    return new List<CursoDTO>();
                }

                var data = await response.Content.ReadFromJsonAsync<List<CursoDTO>>()
                           ?? new List<CursoDTO>();

                
                return data
                    .OrderBy(c => c.Nombre)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consumir la API de cursos (GetAllAsync)");
                throw;
            }
        }
    }
}
