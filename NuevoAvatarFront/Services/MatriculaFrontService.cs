using System.Net.Http.Json;
using System.Text;
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

        Task<(List<Adm19ListadoRowDto> datos, int total)> GetListadoAdm19Async(
            int? idPeriodo,
            int? idCarrera,
            int? idCurso,
            int? idGrupo,
            int page,
            int size,
            string? sort,
            bool asc);

        Task<byte[]> ExportListadoAdm19CsvAsync(
            int? idPeriodo,
            int? idCarrera,
            int? idCurso,
            int? idGrupo,
            string? sort,
            bool asc);

        Task<List<Matricula>> ListarAsync();
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

        public async Task<List<Matricula>> ListarAsync()
        {
            try
            {
                await AttachBearerAsync(_http);

                var idUsuario = ObtenerIdUsuario();

                var url = ConstruirQueryAdm19(
                    idUsuario,
                    idPeriodo: null,
                    idCarrera: null,
                    idCurso: null,
                    idGrupo: null
                );

                var response = await _http.GetAsync(url);

                await EnsureSuccessOrThrowAsync(
                    response,
                    _logger,
                    "No fue posible obtener el listado de matrículas."
                );
                var lista = await response.Content.ReadFromJsonAsync<List<Matricula>>()
                            ?? new List<Matricula>();

                return lista;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al listar matrículas");
                throw;
            }
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

        // ================================
        //   ADM19 - helpers internos
        // ================================
        private string ConstruirQueryAdm19(
            int idUsuario,
            int? idPeriodo,
            int? idCarrera,
            int? idCurso,
            int? idGrupo)
        {
            var qs = new List<string> { $"idUsuario={idUsuario}" };

            if (idPeriodo.HasValue) qs.Add($"idPeriodo={idPeriodo.Value}");
            if (idCarrera.HasValue) qs.Add($"idCarrera={idCarrera.Value}");
            if (idCurso.HasValue) qs.Add($"idCurso={idCurso.Value}");
            if (idGrupo.HasValue) qs.Add($"idGrupo={idGrupo.Value}");

            return "/matricula/adm19" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
        }

        private static IEnumerable<Adm19ListadoRowDto> OrdenarAdm19(
            IEnumerable<Adm19ListadoRowDto> source,
            string? sort,
            bool asc)
        {
            sort = (sort ?? "").ToLowerInvariant();
            IOrderedEnumerable<Adm19ListadoRowDto> ordered;

            switch (sort)
            {
                case "nombre":
                    ordered = asc
                        ? source.OrderBy(x => x.Nombre)
                        : source.OrderByDescending(x => x.Nombre);
                    break;

                case "identificacion":
                    ordered = asc
                        ? source.OrderBy(x => x.Identificacion)
                        : source.OrderByDescending(x => x.Identificacion);
                    break;

                case "carrera":
                    ordered = asc
                        ? source.OrderBy(x => x.Carrera)
                        : source.OrderByDescending(x => x.Carrera);
                    break;

                case "curso":
                    ordered = asc
                        ? source.OrderBy(x => x.Curso)
                        : source.OrderByDescending(x => x.Curso);
                    break;

                case "grupo":
                    ordered = asc
                        ? source.OrderBy(x => x.Numero_Grupo)
                        : source.OrderByDescending(x => x.Numero_Grupo);
                    break;

                default: // id
                    ordered = asc
                        ? source.OrderBy(x => x.ID_Matricula)
                        : source.OrderByDescending(x => x.ID_Matricula);
                    break;
            }

            return ordered;
        }

        // ================================
        //   ADM19 - listado paginado
        // ================================
        public async Task<(List<Adm19ListadoRowDto> datos, int total)> GetListadoAdm19Async(
            int? idPeriodo,
            int? idCarrera,
            int? idCurso,
            int? idGrupo,
            int page,
            int size,
            string? sort,
            bool asc)
        {
            try
            {
                await AttachBearerAsync(_http);

                var idUsuario = ObtenerIdUsuario();
                var url = ConstruirQueryAdm19(idUsuario, idPeriodo, idCarrera, idCurso, idGrupo);

                var response = await _http.GetAsync(url);

                await EnsureSuccessOrThrowAsync(
                    response,
                    _logger,
                    "No fue posible obtener el listado de matrículas."
                );

                var lista =
                    await response.Content.ReadFromJsonAsync<List<Adm19ListadoRowDto>>()
                    ?? new List<Adm19ListadoRowDto>();

                var total = lista.Count;

                if (total == 0)
                    return (new List<Adm19ListadoRowDto>(), 0);

                var ordenados = OrdenarAdm19(lista, sort, asc);

                if (size <= 0) size = 10;
                if (page <= 0) page = 1;

                var skip = (page - 1) * size;
                var pagina = ordenados
                    .Skip(skip)
                    .Take(size)
                    .ToList();

                return (pagina, total);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener listado ADM19");
                throw;
            }
        }

        // ================================
        //   ADM19 - exportar CSV
        // ================================
        public async Task<byte[]> ExportListadoAdm19CsvAsync(
            int? idPeriodo,
            int? idCarrera,
            int? idCurso,
            int? idGrupo,
            string? sort,
            bool asc)
        {
            try
            {
                await AttachBearerAsync(_http);

                var idUsuario = ObtenerIdUsuario();
                var url = ConstruirQueryAdm19(idUsuario, idPeriodo, idCarrera, idCurso, idGrupo);

                var response = await _http.GetAsync(url);

                await EnsureSuccessOrThrowAsync(
                    response,
                    _logger,
                    "No fue posible exportar el listado a CSV."
                );

                var lista =
                    await response.Content.ReadFromJsonAsync<List<Adm19ListadoRowDto>>()
                    ?? new List<Adm19ListadoRowDto>();

                var ordenados = OrdenarAdm19(lista, sort, asc).ToList();

                var sb = new StringBuilder();
                sb.AppendLine("ID_Matricula;ID_Estudiante;Tipo_Identificacion;Identificacion;Nombre;ID_Carrera;Carrera;ID_Curso;Curso;ID_Grupo;Numero_Grupo;ID_Periodo");

                foreach (var r in ordenados)
                {
                    sb.AppendLine(
                        $"{r.ID_Matricula};{r.ID_Estudiante};{r.Tipo_Identificacion};{r.Identificacion};{r.Nombre};" +
                        $"{r.ID_Carrera};{r.Carrera};{r.ID_Curso};{r.Curso};{r.ID_Grupo};{r.Numero_Grupo};{r.ID_Periodo}");
                }

                return Encoding.UTF8.GetBytes(sb.ToString());
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al exportar CSV ADM19");
                throw;
            }
        }
    }
}
