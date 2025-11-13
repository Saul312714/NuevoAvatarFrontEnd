using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;

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

        // Si tienes otra forma de obtener el usuario, cámbialo aquí
        private const int ID_USUARIO_DEFECTO = 5;

        public MatriculaFrontService(
            IHttpClientFactory factory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<MatriculaFrontService> logger)
            : base(httpContextAccessor)
        {
            _http = factory.CreateClient("MatriculaClient");
            _logger = logger;
        }

        private int ObtenerIdUsuario()
        {
            // Aquí puedes luego leer de Session o de Claims.
            // Por ahora dejamos el 5 como en tus pruebas.
            return ID_USUARIO_DEFECTO;
        }

        public async Task<int> CrearAsync(MatriculaCreateDto dto)
        {
            AttachBearer(_http);
            var idUsuario = ObtenerIdUsuario();

            var response = await _http.PostAsJsonAsync($"/matricula?idUsuario={idUsuario}", dto);

            if (response.IsSuccessStatusCode)
            {
                // Tu endpoint devuelve la matrícula creada (Matricula)
                var creada = await response.Content.ReadFromJsonAsync<Matricula>();
                return creada?.ID_Matricula ?? 0;
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error al crear matrícula. Status: {Status}. Body: {Body}",
                response.StatusCode, error);

            throw new Exception($"Error al crear matrícula: {error}");
        }

        public async Task<List<MatriculadoDto>> GetMatriculadosPorCursoGrupoAsync(int idCurso, int idGrupo)
        {
            AttachBearer(_http);
            var idUsuario = ObtenerIdUsuario();

            var response = await _http.GetAsync($"/matricula/curso/{idCurso}/grupo/{idGrupo}?idUsuario={idUsuario}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Tu endpoint devuelve 404 cuando no hay matriculados
                return new List<MatriculadoDto>();
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al obtener matriculados. Status: {Status}. Body: {Body}",
                    response.StatusCode, error);

                throw new Exception($"Error al consultar matriculados: {error}");
            }

            var lista = await response.Content.ReadFromJsonAsync<List<MatriculadoDto>>();
            return lista ?? new List<MatriculadoDto>();
        }
    }
}