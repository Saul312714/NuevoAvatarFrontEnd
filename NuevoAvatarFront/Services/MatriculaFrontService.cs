using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Helpers;

namespace NuevoAvatarFront.Services
{
    public interface IMatriculaFrontService
    {
        Task<int> CrearAsync(MatriculaCreateDto dto);
        Task<List<MatriculadoDto>> GetMatriculadosPorCursoGrupoAsync(int idCurso, int idGrupo);
    }

    /// <summary>
    /// Cliente de la API PV_NA_Matricula para la HU de matrícula:
    /// - POST /matricula?idUsuario=...
    /// - GET  /matricula/curso/{idCurso}/grupo/{idGrupo}?idUsuario=...
    /// </summary>
    public class MatriculaFrontService : ApiClientBase, IMatriculaFrontService
    {
        private readonly HttpClient _http;
        private readonly ILogger<MatriculaFrontService> _logger;

        public MatriculaFrontService(
            IHttpClientFactory factory,
            IHttpContextAccessor httpContextAccessor,
            TokenHelper tokenHelper,
            ILogger<MatriculaFrontService> logger
        ) : base(httpContextAccessor, tokenHelper)
        {
            _http = factory.CreateClient("MatriculaClient");
            _logger = logger;
        }

        private int ObtenerIdUsuario()
        {
            // 🔥 Ahora obtenemos el UsuarioID REAL desde la sesión del login
            var id = _httpContextAccessor.HttpContext?.Session.GetInt32("UsuarioID") ?? 0;

            if (id == 0)
            {
                _logger.LogWarning("⚠ UsuarioID no encontrado en sesión, posible sesión expirada.");
            }

            return id;
        }

        public async Task<int> CrearAsync(MatriculaCreateDto dto)
        {
            try
            {
                // 🔥 Adjunta AccessToken válido
                await AttachBearerAsync(_http);

                var idUsuario = ObtenerIdUsuario();

                var response = await _http.PostAsJsonAsync($"/matricula?idUsuario={idUsuario}", dto);

                if (response.IsSuccessStatusCode)
                {
                    // Tu endpoint devuelve un objeto Matricula
                    var creada = await response.Content.ReadFromJsonAsync<Matricula>();
                    return creada?.ID_Matricula ?? 0;
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al crear matrícula. Status: {Status}. Body: {Body}",
                    response.StatusCode, error);

                throw new Exception($"Error al crear matrícula: {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear matrícula");
                throw;
            }
        }

        public async Task<List<MatriculadoDto>> GetMatriculadosPorCursoGrupoAsync(int idCurso, int idGrupo)
        {
            try
            {
                
                await AttachBearerAsync(_http);

                var idUsuario = ObtenerIdUsuario();

                var response = await _http.GetAsync(
                    $"/matricula/curso/{idCurso}/grupo/{idGrupo}?idUsuario={idUsuario}"
                );

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    
                    return new List<MatriculadoDto>();
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Error al obtener matriculados. Status: {Status}. Body: {Body}",
                        response.StatusCode, error
                    );

                    throw new Exception($"Error al consultar matriculados: {error}");
                }

                var lista = await response.Content.ReadFromJsonAsync<List<MatriculadoDto>>();
                return lista ?? new List<MatriculadoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener matriculados");
                throw;
            }
        }
    }
}
