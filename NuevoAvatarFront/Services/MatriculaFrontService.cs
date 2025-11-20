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
    public interface IMatriculaFrontService
    {
        Task<int> CrearAsync(MatriculaCreateDto dto);
        Task<List<MatriculadoDto>> GetMatriculadosPorCursoGrupoAsync(int idCurso, int idGrupo);
    }

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
                await AttachBearerAsync(_http);

                var idUsuario = ObtenerIdUsuario();

                var response = await _http.PostAsJsonAsync(
                    $"/matricula?idUsuario={idUsuario}",
                    dto
                );

                if (response.IsSuccessStatusCode)
                {
                    var creada = await response.Content.ReadFromJsonAsync<Matricula>();
                    return creada?.ID_Matricula ?? 0;
                }

                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Error al crear matrícula. Status: {Status}. Body: {Body}",
                    response.StatusCode, body
                );

                string mensajeUsuario =
                    "No fue posible registrar la matrícula. Verifique los datos e inténtelo de nuevo.";

                List<string>? errors = null;

                if (!string.IsNullOrWhiteSpace(body))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(body);
                        var root = doc.RootElement;
                        string? msg = null;

                        if (root.ValueKind == JsonValueKind.Object)
                        {
                            if (root.TryGetProperty("message", out var m))
                                msg = m.GetString();
                            else if (root.TryGetProperty("Message", out var m2))
                                msg = m2.GetString();
                            else if (root.TryGetProperty("error", out var e))
                                msg = e.GetString();
                            else if (root.TryGetProperty("Error", out var e2))
                                msg = e2.GetString();
                        }
                        else if (root.ValueKind == JsonValueKind.String)
                        {
                            msg = root.GetString();
                        }

                        if (!string.IsNullOrWhiteSpace(msg))
                        {
                            mensajeUsuario = msg!;
                        }

                        errors = new List<string> { mensajeUsuario };
                    }
                    catch
                    {
                        var plain = body.Length > 500
                            ? body.Substring(0, 500) + "..."
                            : body;

                        if (!string.IsNullOrWhiteSpace(plain))
                            mensajeUsuario = plain;

                        errors = new List<string> { mensajeUsuario };
                    }
                }

                throw new ApiException(
                    mensajeUsuario,
                    response.StatusCode,
                    errors
                );
            }
            catch (ApiException)
            {
                throw;
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

                await EnsureSuccessOrThrowAsync(
                    response,
                    _logger,
                    "No fue posible obtener los estudiantes matriculados para ese curso y grupo."
                );

                var lista = await response.Content.ReadFromJsonAsync<List<MatriculadoDto>>();
                return lista ?? new List<MatriculadoDto>();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener matriculados");
                throw;
            }
        }
    }
}
