using System.Net.Http.Json;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;

namespace NuevoAvatarFront.Services
{
    public interface IEstudianteService
    {
        Task<List<EstudianteDto>> GetAllAsync();
    }

    public class EstudianteService : ApiClientBase, IEstudianteService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EstudianteService> _logger;

        public EstudianteService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<EstudianteService> logger)
            : base(httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("MatriculaClient");
            _configuration = configuration;
            _logger = logger;
        }

        private int ObtenerIdUsuario()
        {
            var userId = _configuration["Auth:DevUserId"];
            return int.TryParse(userId, out var id) ? id : 0;
        }

        public async Task<List<EstudianteDto>> GetAllAsync()
        {
            try
            {
                AttachBearer(_httpClient);
                var idUsuario = ObtenerIdUsuario();

                
                var response = await _httpClient.GetAsync($"/expediente?idUsuario={idUsuario}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<EstudianteDto>>();
                    return data ?? new List<EstudianteDto>();
                }

                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Error al obtener estudiantes. Status: {Status}, Error: {Error}",
                    response.StatusCode, err);

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