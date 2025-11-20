using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Helpers;

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
            TokenHelper tokenHelper,
            ILogger<CursoService> logger
        ) : base(httpContextAccessor, tokenHelper)
        {
            _http = httpClientFactory.CreateClient("OfertaClient");
            _logger = logger;
        }

        public async Task<List<CursoDTO>> GetNivel1Async()
        {
            try
            {
                await AttachBearerAsync(_http);

                var resp = await _http.GetAsync("/curso");

                await EnsureSuccessOrThrowAsync(
                    resp,
                    _logger,
                    "No fue posible obtener la lista de cursos de nivel 1."
                );

                if (!resp.IsSuccessStatusCode)
                {
                    var error = await resp.Content.ReadAsStringAsync();
                    _logger.LogError("Error al obtener cursos. Status: {Status}, Error: {Error}",
                        resp.StatusCode, error);

                    return new List<CursoDTO>();
                }

                var data = await resp.Content.ReadFromJsonAsync<List<CursoDTO>>()
                           ?? new List<CursoDTO>();

                return data
                    .Where(c => c.Nivel == 1)
                    .OrderBy(c => c.Nombre)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consumir la API de cursos (GetNivel1Async)");
                throw;
            }
        }

        public async Task<List<CursoDTO>> GetNivel1PorCarreraAsync(int idCarrera)
        {
            try
            {
                await AttachBearerAsync(_http);

                var resp = await _http.GetAsync("/curso");

                await EnsureSuccessOrThrowAsync(
                    resp,
                    _logger,
                    "No fue posible obtener los cursos de nivel 1 por carrera."
                );

                if (!resp.IsSuccessStatusCode)
                {
                    var error = await resp.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Error al obtener cursos por carrera. Status: {Status}, Error: {Error}",
                        resp.StatusCode, error
                    );

                    return new List<CursoDTO>();
                }

                var data = await resp.Content.ReadFromJsonAsync<List<CursoDTO>>()
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
                await AttachBearerAsync(_http);

                var resp = await _http.GetAsync("/curso");

                await EnsureSuccessOrThrowAsync(
                    resp,
                    _logger,
                    "No fue posible obtener todos los cursos."
                );

                if (!resp.IsSuccessStatusCode)
                {
                    var error = await resp.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Error al obtener todos los cursos. Status: {Status}, Error: {Error}",
                        resp.StatusCode, error
                    );

                    return new List<CursoDTO>();
                }

                var data = await resp.Content.ReadFromJsonAsync<List<CursoDTO>>()
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
