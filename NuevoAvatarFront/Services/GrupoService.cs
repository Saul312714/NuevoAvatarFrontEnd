using System.Net.Http.Json;
using NuevoAvatarFront.Models;

namespace NuevoAvatarFront.Services
{
    public interface IGrupoService
    {
        Task<List<GrupoDto>> GetAllAsync();
        Task<List<GrupoDto>> GetByCursoAsync(int idCurso);
    }

    public class GrupoService : IGrupoService
    {
        private readonly HttpClient _http;
        private readonly ILogger<GrupoService> _logger;

        public GrupoService(
            IHttpClientFactory httpClientFactory,
            ILogger<GrupoService> logger)
        {
            
            _http = httpClientFactory.CreateClient("OfertaClient");
            _logger = logger;
        }

        public async Task<List<GrupoDto>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync("/grupo");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content
                        .ReadFromJsonAsync<List<GrupoDto>>();

                    return data ?? new List<GrupoDto>();
                }

                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al obtener grupos. Status: {Status}. Body: {Body}",
                    response.StatusCode, err);

                return new List<GrupoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado al obtener grupos");
                throw;
            }
        }

        // Filtro por curso (usando los datos obtenidos de GetAllAsync)
        public async Task<List<GrupoDto>> GetByCursoAsync(int idCurso)
        {
            var todos = await GetAllAsync();
            return todos
                .Where(g => g.ID_Curso == idCurso)
                .OrderBy(g => g.Numero_Grupo)
                .ToList();
        }
    }
}