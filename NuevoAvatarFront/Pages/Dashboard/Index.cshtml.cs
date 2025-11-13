using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;

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


        public string NombreUsuario { get; set; } = "Usuario";
        public string RolUsuario { get; set; } = "Administrador";

        public int TotalUsuarios { get; set; }
        public int TotalCarreras { get; set; }
        public int TotalCursos { get; set; }
        public int TotalFacturasPendientes { get; set; }

        public List<Notificacion> Notificaciones { get; set; } = new();
        public List<Bitacora> Bitacoras { get; set; } = new();

        public async Task OnGetAsync()
        {
            var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1IiwiZW1haWwiOiJhbmRyZXMudmlsbGFsb2Jvc0BjdWMuYWMuY3IiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIyIiwiZXhwIjoxNzYzMDEzNDkxLCJpc3MiOiJQVl9OQSIsImF1ZCI6IlBWX05BLkNsaWVudCJ9.ejK7gm2pfuFZtEktwos2URfB0Hu4wSXnxC3-sNjaBnA";

            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            var idUsuarioAccion = jsonToken?.Claims.FirstOrDefault(c => c.Type == "ID_Usuario")?.Value;

            if (idUsuarioAccion == null)
            {
                idUsuarioAccion = "5";
            }

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            TotalUsuarios = await ContarRegistros($"{_config["ApiUsuariosRoles"]}/usuario?idUsuarioAccion={idUsuarioAccion}");

            TotalCarreras = await ContarRegistros($"{_config["ApiOfertaAcademica"]}/carrera");
            TotalCursos = await ContarRegistros($"{_config["ApiOfertaAcademica"]}/curso");
            TotalFacturasPendientes = await ContarRegistros($"{_config["ApiPagos"]}/factura");

            Notificaciones = await ObtenerLista<Notificacion>($"{_config["ApiNotificaciones"]}/notificar");
            Bitacoras = await ObtenerLista<Bitacora>($"{_config["ApiSeguridad"]}/bitacora");

            await RegistrarBitacora(int.Parse(idUsuarioAccion), "Visualización del dashboard");
        }




        private async Task<int> ContarRegistros(string url)
        {
            try
            {
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode) return 0;

                var json = await response.Content.ReadAsStringAsync();
                var lista = JsonSerializer.Deserialize<List<object>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return lista?.Count ?? 0;
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
                var list = JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return list?.Take(5).ToList() ?? new List<T>();
            }
            catch
            {
                return new List<T>();
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
                // No interrumpir el flujo si falla el registro
            }
        }
    }


    public class Notificacion
    {
        public int ID_Notificacion { get; set; }
        public string email_Destino { get; set; } = "";
        public string asunto { get; set; } = "";
        public string cuerpo { get; set; } = "";
        public DateTime fechaEnvio { get; set; }
        public string estado { get; set; } = "";
    }

    public class Bitacora
    {
        public int ID_Bitacora { get; set; }
        public int ID_Usuario { get; set; }
        public string accion { get; set; } = "";
        public DateTime fecha { get; set; }
    }
}
