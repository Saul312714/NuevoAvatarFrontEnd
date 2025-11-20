using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace NuevoAvatarFront.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public IndexModel(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient();
            _config = config;
        }

        // Información del usuario
        public string NombreUsuario { get; set; } = "Usuario";
        public string RolUsuario { get; set; } = "Rol";
        public int IdUsuario { get; set; }

        // Métricas para ADMINISTRADOR
        public int TotalUsuarios { get; set; }
        public int TotalCarreras { get; set; }
        public int TotalCursos { get; set; }
        public int TotalPeriodosActivos { get; set; }
        public int TotalFacturasPendientes { get; set; }

        // Métricas para ESTUDIANTE
        public int MisMatriculas { get; set; }
        public int MisCursosActuales { get; set; }
        public decimal MiSaldoPendiente { get; set; }
        public string MiCarrera { get; set; } = "";

        // Métricas para PROFESOR
        public int MisGrupos { get; set; }
        public int CursosQueImparto { get; set; }
        public List<GrupoProfesor> MisGruposDetalle { get; set; } = new();

        // Datos comunes
        public List<Notificacion> Notificaciones { get; set; } = new();
        public List<Bitacora> Bitacoras { get; set; } = new();

        public async Task OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (string.IsNullOrWhiteSpace(token) || userId == null)
            {
                Response.Redirect("/HU_MICHEL/Login");
                return;
            }

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            IdUsuario = userId.Value;
            NombreUsuario = HttpContext.Session.GetString("UserName") ?? "Usuario";
            RolUsuario = HttpContext.Session.GetString("UserRole") ?? "Rol";

            // Cargar datos según el rol
            if (RolUsuario.Contains("Administrador") || RolUsuario.Contains("Admin"))
            {
                await CargarDatosAdministrador();
            }
            else if (RolUsuario.Contains("Estudiante"))
            {
                await CargarDatosEstudiante();
            }
            else if (RolUsuario.Contains("Profesor"))
            {
                await CargarDatosProfesor();
            }

            // Datos comunes para todos los roles
            await CargarDatosComunes();

            // Registrar en bitácora
            await RegistrarBitacora(IdUsuario, $"Visualización del dashboard - Rol: {RolUsuario}");
        }

        // ============================================================
        // DATOS PARA ADMINISTRADOR
        // ============================================================
        private async Task CargarDatosAdministrador()
        {
            TotalUsuarios = await ContarRegistros($"{_config["ApiUsuariosRoles"]}/usuario?idUsuarioAccion={IdUsuario}");
            TotalCarreras = await ContarRegistros($"{_config["ApiOfertaAcademica"]}/carrera");
            TotalCursos = await ContarRegistros($"{_config["ApiOfertaAcademica"]}/curso");
            TotalPeriodosActivos = await ContarRegistros($"{_config["ApiOfertaAcademica"]}/periodo?idUsuarioAccion={IdUsuario}");
            TotalFacturasPendientes = await ContarFacturasPendientes();
        }

        // ============================================================
        // DATOS PARA ESTUDIANTE
        // ============================================================
        private async Task CargarDatosEstudiante()
        {
            try
            {
                // Obtener expediente del estudiante
                var expediente = await ObtenerObjeto<Expediente>(
                    $"{_config["ApiMatricula"]}/expediente/usuario/{IdUsuario}?idUsuario={IdUsuario}");

                if (expediente != null)
                {
                    // Contar matrículas del estudiante
                    MisMatriculas = await ContarRegistros(
                        $"{_config["ApiMatricula"]}/matricula/estudiante/{expediente.ID_Estudiante}?idUsuario={IdUsuario}");

                    // Obtener carrera (si existe)
                    MiCarrera = "No asignada";
                }

                // Obtener saldo pendiente de facturas
                MiSaldoPendiente = await ObtenerSaldoPendiente();

                // Cursos actuales (aproximado por matrículas activas)
                MisCursosActuales = MisMatriculas;
            }
            catch
            {
                // En caso de error, dejar valores por defecto
            }
        }

        // ============================================================
        // DATOS PARA PROFESOR
        // ============================================================
        private async Task CargarDatosProfesor()
        {
            try
            {
                // Obtener grupos del profesor
                var grupos = await ObtenerLista<dynamic>(
                    $"{_config["ApiOfertaAcademica"]}/grupo/profesor/{IdUsuario}");

                MisGrupos = grupos.Count;

                // Contar cursos únicos
                var cursosUnicos = new HashSet<int>();
                foreach (var grupo in grupos)
                {
                    try
                    {
                        if (grupo.GetProperty("id_Curso").TryGetInt32(out int idCurso))
                        {
                            cursosUnicos.Add(idCurso);
                        }
                    }
                    catch { }
                }

                CursosQueImparto = cursosUnicos.Count;
            }
            catch
            {
                // En caso de error, dejar valores por defecto
            }
        }

        // ============================================================
        // DATOS COMUNES
        // ============================================================
        private async Task CargarDatosComunes()
        {
            Notificaciones = await ObtenerLista<Notificacion>(
                $"{_config["ApiNotificaciones"]}/notificar");

            Bitacoras = await ObtenerLista<Bitacora>(
                $"{_config["ApiSeguridad"]}/bitacora");
        }

        // ============================================================
        // MÉTODOS AUXILIARES
        // ============================================================
        private async Task<int> ContarRegistros(string url)
        {
            try
            {
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return 0;

                var json = await response.Content.ReadAsStringAsync();
                var lista = JsonSerializer.Deserialize<List<object>>(json);

                return lista?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> ContarFacturasPendientes()
        {
            try
            {
                var facturas = await ObtenerLista<dynamic>($"{_config["ApiPagos"]}/factura");
                return facturas.Count(f =>
                {
                    try
                    {
                        var estado = f.GetProperty("estado").GetString();
                        return estado == "Pendiente" || estado == "pendiente";
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
            catch
            {
                return 0;
            }
        }

        private async Task<decimal> ObtenerSaldoPendiente()
        {
            try
            {
                var facturas = await ObtenerLista<dynamic>(
                    $"{_config["ApiPagos"]}/factura/estudiante/{IdUsuario}");

                decimal total = 0;
                foreach (var f in facturas)
                {
                    try
                    {
                        var estado = f.GetProperty("estado").GetString();
                        if (estado == "Pendiente" || estado == "pendiente")
                        {
                            total += f.GetProperty("monto").GetDecimal();
                        }
                    }
                    catch { }
                }
                return total;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<List<T>> ObtenerLista<T>(string url)
        {
            try
            {
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new List<T>();

                var json = await response.Content.ReadAsStringAsync();
                var list = JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return list?.Take(5).ToList() ?? new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        private async Task<T?> ObtenerObjeto<T>(string url)
        {
            try
            {
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return default;

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return default;
            }
        }

        private async Task RegistrarBitacora(int idUsuario, string accion)
        {
            var bitacoraUrl = $"{_config["ApiSeguridad"]}/bitacora";

            var body = new
            {
                ID_Usuario = idUsuario,
                Fecha = DateTime.Now,
                Accion = accion
            };

            try
            {
                await _http.PostAsJsonAsync(bitacoraUrl, body);
            }
            catch
            {
                // No romper ejecución si la bitácora falla
            }
        }
    }

    // ============================================================
    // DTOs
    // ============================================================
    public class Notificacion
    {
        public int ID_Notificacion { get; set; }
        public string Email_Destino { get; set; } = "";
        public string Asunto { get; set; } = "";
        public string Cuerpo { get; set; } = "";
        public DateTime FechaEnvio { get; set; }
        public string Estado { get; set; } = "";
    }

    public class Bitacora
    {
        public int ID_Bitacora { get; set; }
        public int ID_Usuario { get; set; }
        public string Accion { get; set; } = "";
        public DateTime Fecha { get; set; }
    }

    public class Expediente
    {
        public int ID_Estudiante { get; set; }
        public string Identificacion { get; set; } = "";
        public string Nombre { get; set; } = "";
    }

    public class GrupoProfesor
    {
        public int ID_Grupo { get; set; }
        public string NombreCurso { get; set; } = "";
        public string Horario { get; set; } = "";
    }
}