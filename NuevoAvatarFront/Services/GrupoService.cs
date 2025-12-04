using System.Net.Http.Json;
using System.Text.Json;                         
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;

namespace NuevoAvatarFront.Services
{
    public interface IGrupoService
    {
        Task<List<GrupoDto>> GetAllAsync();
        Task<List<GrupoDto>> GetByCursoAsync(int idCurso);
       
    }

    public class GrupoService : ApiClientBase, IGrupoService
    {
        private readonly HttpClient _http;
        private readonly ILogger<GrupoService> _logger;

        public GrupoService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            TokenHelper tokenHelper,
            ILogger<GrupoService> logger
        ) : base(httpContextAccessor, tokenHelper)
        {
            _http = httpClientFactory.CreateClient("OfertaClient");
            _logger = logger;
        }

        public async Task<List<GrupoDto>> GetAllAsync()
        {
            try
            {
                await AttachBearerAsync(_http);

                var response = await _http.GetAsync("/grupo");

                await EnsureSuccessOrThrowAsync(
                    response,
                    _logger,
                    "No fue posible obtener el listado de grupos."
                );

                if (response.IsSuccessStatusCode)
                {
                    var opciones = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var data = await response.Content
                        .ReadFromJsonAsync<List<GrupoDto>>(opciones);

                    return data ?? new List<GrupoDto>();
                }

                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Error al obtener grupos. Status: {Status}. Body: {Body}",
                    response.StatusCode, err
                );

                return new List<GrupoDto>();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado al obtener grupos");
                throw;
            }
        }

        public async Task<List<GrupoDto>> GetByCursoAsync(int idCurso)
        {
            try
            {
                var todos = await GetAllAsync();

                return todos
                    .Where(g => g.ID_Curso == idCurso)
                    .OrderBy(g => g.Numero_Grupo)
                    .ToList();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al filtrar grupos por curso {CursoId}", idCurso);
                throw;
            }
        }
    }
}

