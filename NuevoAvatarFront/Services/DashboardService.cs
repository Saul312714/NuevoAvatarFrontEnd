using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;
using NuevoAvatarFront.Entities.Dashboard;

namespace NuevoAvatarFront.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public DashboardService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        private void AddToken(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<DashboardData> LoadDashboard(int idUsuario, string rolUsuario, string token)
        {
            AddToken(token);

            var data = new DashboardData
            {
                IdUsuario = idUsuario,
                RolUsuario = rolUsuario
            };

            // Cargar datos según rol
            if (rolUsuario.Contains("Admin"))
                await LoadAdminData(data);

            else if (rolUsuario.Contains("Estudiante"))
                await LoadStudentData(data);

            else if (rolUsuario.Contains("Profesor"))
                await LoadProfesorData(data);

            // Datos comunes
            await LoadCommonData(data);

            if (rolUsuario.Contains("Admin"))
            {
                data.Bitacoras = await GetList<BitacoraDto>(
                    $"{_config["BitacoraApi:BaseUrl"]}/bitacora"
                );
            }
            else
            {
                data.Bitacoras = new(); // Vacío para usuarios que no son admin
            }

            // Registrar actividad
            await RegistrarBitacora(idUsuario, $"Acceso al dashboard ({rolUsuario})");

            return data;
        }



        private async Task LoadAdminData(DashboardData data)
        {
            data.TotalUsuarios = await Contar($"{_config["UsuariosRolesApi:BaseUrl"]}/usuario?idUsuarioAccion={data.IdUsuario}");
            data.TotalCarreras = await Contar($"{_config["OfertaAcademicaApi:BaseUrl"]}/carrera");
            data.TotalCursos = await Contar($"{_config["OfertaAcademicaApi:BaseUrl"]}/curso");
            data.TotalPeriodosActivos = await Contar($"{_config["OfertaAcademicaApi:BaseUrl"]}/periodo?idUsuarioAccion={data.IdUsuario}");
            data.TotalFacturasPendientes = await ContarFacturasPendientes();

            data.Bitacoras = await GetFullList<BitacoraDto>(
                $"{_config["BitacoraApi:BaseUrl"]}/bitacora");

            data.Usuarios = await GetFullList<UsuarioSimpleDto>(
                $"{_config["UsuariosRolesApi:BaseUrl"]}/usuario?idUsuarioAccion={data.IdUsuario}");

            data.UsuariosLookup = data.Usuarios
                .ToDictionary(u => u.ID_Usuario, u => u.Nombre);
        }


        private async Task LoadStudentData(DashboardData data)
        {
            try
            {
                var expediente = await GetObject<dynamic>(
                    $"{_config["MatriculaApi:BaseUrl"]}/expediente/usuario/{data.IdUsuario}?idUsuario={data.IdUsuario}");

                if (expediente != null)
                {
                    int idEstudiante = expediente.GetProperty("id_Estudiante").GetInt32();

                    data.MisMatriculas = await Contar(
                        $"{_config["MatriculaApi:BaseUrl"]}/matricula/estudiante/{idEstudiante}?idUsuario={data.IdUsuario}");

                    data.MisCursosActuales = data.MisMatriculas;
                }

                data.MiSaldoPendiente = await ObtenerSaldoPendiente();
            }
            catch { }
        }


        private async Task LoadProfesorData(DashboardData data)
        {
            try
            {
                var grupos = await GetList<dynamic>(
                    $"{_config["MatriculaApi:BaseUrl"]}/grupo/profesor/{data.IdUsuario}");

                data.MisGrupos = grupos.Count;

                var cursosUnicos = new HashSet<int>();

                foreach (var g in grupos)
                {
                    try
                    {
                        cursosUnicos.Add(g.GetProperty("id_Curso").GetInt32());
                    }
                    catch { }
                }

                data.CursosQueImparto = cursosUnicos.Count;
            }
            catch { }
        }

        private async Task LoadCommonData(DashboardData data)
        {
            data.Notificaciones = await GetList<NotificacionDto>(
                $"{_config["NotificacionesApi:BaseUrl"]}/notificar");

            // Si NO es admin → bitácoras solo recientes
            if (!data.RolUsuario.Contains("Admin"))
            {
                data.Bitacoras = await GetList<BitacoraDto>(
                    $"{_config["BitacoraApi:BaseUrl"]}/bitacora");
            }
        }


        private async Task<int> Contar(string url)
        {
            try
            {
                var list = await GetList<object>(url);
                return list.Count;
            }
            catch { return 0; }
        }

        private async Task<int> ContarFacturasPendientes()
        {
            var facturas = await GetList<dynamic>($"{_config["PagosApi:BaseUrl"]}/factura");

            return facturas.Count(f =>
            {
                try
                {
                    var estado = f.GetProperty("estado").GetString();
                    return estado?.ToLower() == "pendiente";
                }
                catch { return false; }
            });
        }

        private async Task<decimal> ObtenerSaldoPendiente()
        {
            var facturas = await GetList<dynamic>(
                $"{_config["PagosApi:BaseUrl"]}/factura/estudiante");

            decimal total = 0;

            foreach (var f in facturas)
            {
                try
                {
                    var estado = f.GetProperty("estado").GetString();

                    if (estado?.ToLower() == "pendiente")
                        total += f.GetProperty("monto").GetDecimal();
                }
                catch { }
            }

            return total;
        }

        // Lista limitada a 5 (para estudiantes y profesores)
        private async Task<List<T>> GetList<T>(string url)
        {
            try
            {
                var result = await _http.GetFromJsonAsync<List<T>>(url);
                return result?.Take(5).ToList() ?? new List<T>();
            }
            catch { return new List<T>(); }
        }

        // Lista completa (solo para Admin)
        private async Task<List<T>> GetFullList<T>(string url)
        {
            try
            {
                var result = await _http.GetFromJsonAsync<List<T>>(url);
                return result ?? new List<T>();
            }
            catch { return new List<T>(); }
        }

        // Para objetos individuales
        private async Task<T?> GetObject<T>(string url)
        {
            try
            {
                return await _http.GetFromJsonAsync<T>(url);
            }
            catch { return default; }
        }

        // Registrar en bitácora
        private async Task RegistrarBitacora(int idUsuario, string accion)
        {
            try
            {
                await _http.PostAsJsonAsync($"{_config["BitacoraApi:BaseUrl"]}/bitacora", new
                {
                    ID_Usuario = idUsuario,
                    Fecha = DateTime.Now,
                    Accion = accion
                });
            }
            catch { }
        }
    }
}
