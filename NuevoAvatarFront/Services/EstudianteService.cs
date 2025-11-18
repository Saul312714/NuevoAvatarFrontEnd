using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Helpers;

namespace NuevoAvatarFront.Services
{
    public interface IEstudianteService
    {
        Task<List<EstudianteDto>> GetAllAsync();
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
                    _logger.LogWarning("⚠ No se encontró UsuarioID en sesión. Devolviendo lista vacía.");
                    return new List<EstudianteDto>();
                }

                var response = await _httpClient.GetAsync($"/expediente?idUsuario={idUsuario}");

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
    }
}
