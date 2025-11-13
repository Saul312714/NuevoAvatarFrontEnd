using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NuevoAvatarFront.Pages.Expedientes
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

        public List<ExpedienteViewDto> Expedientes { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? FiltroIdentificacion { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroNombre { get; set; }

        // ============================================================
        // GET
        // ============================================================
        public async Task OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (string.IsNullOrWhiteSpace(token) || userId == null)
            {
                Response.Redirect("/HU_MICHEL/Login");
                return;
            }

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = $"{_config["ApiMatricula"]}/expediente?idUsuario={userId}";
            var lista = await _http.GetFromJsonAsync<List<ExpedienteDto>>(url) ?? new();

            // ===================================================================
            // SEPARACIÓN DE DIRECCIÓN: "provincia - canton - distrito"
            // ===================================================================
            var listaView = new List<ExpedienteViewDto>();

            foreach (var e in lista)
            {
                string provincia = "";
                string canton = "";
                string distrito = "";

                if (!string.IsNullOrWhiteSpace(e.Direccion))
                {
                    var partes = e.Direccion.Split(" - ");
                    if (partes.Length == 3)
                    {
                        provincia = partes[0];
                        canton = partes[1];
                        distrito = partes[2];
                    }
                }

                listaView.Add(new ExpedienteViewDto
                {
                    ID_Estudiante = e.ID_Estudiante,
                    Identificacion = e.Identificacion,
                    Tipo_Identificacion = e.Tipo_Identificacion,
                    Nombre = e.Nombre,
                    Email = e.Email,
                    Telefono = e.Telefono,
                    Fecha_Nacimiento = e.Fecha_Nacimiento,

                    Provincia = provincia,
                    Canton = canton,
                    Distrito = distrito
                });
            }

            // FILTROS
            if (!string.IsNullOrWhiteSpace(FiltroIdentificacion))
                listaView = listaView
                    .Where(e => e.Identificacion.Contains(FiltroIdentificacion))
                    .ToList();

            if (!string.IsNullOrWhiteSpace(FiltroNombre))
                listaView = listaView
                    .Where(e => e.Nombre.Contains(FiltroNombre, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            Expedientes = listaView;
        }

        // ============================================================
        // DELETE
        // ============================================================
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (string.IsNullOrWhiteSpace(token) || userId == null)
                return Redirect("/HU_MICHEL/Login");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            await _http.DeleteAsync(
                $"{_config["ApiMatricula"]}/expediente/{id}?idUsuario={userId}");

            return RedirectToPage(new { FiltroIdentificacion, FiltroNombre });
        }


        // ============================================================
        // DTOs
        // ============================================================

        // DTO recibido desde la API
        public class ExpedienteDto
        {
            public int ID_Estudiante { get; set; }
            public string Identificacion { get; set; } = "";
            public string Tipo_Identificacion { get; set; } = "";
            public string Nombre { get; set; } = "";
            public string Email { get; set; } = "";
            public DateTime Fecha_Nacimiento { get; set; }
            public string Telefono { get; set; } = "";
            public string Direccion { get; set; } = "";  // "provincia - canton - distrito"
        }

        // DTO para mostrar en pantalla
        public class ExpedienteViewDto
        {
            public int ID_Estudiante { get; set; }
            public string Identificacion { get; set; } = "";
            public string Tipo_Identificacion { get; set; } = "";
            public string Nombre { get; set; } = "";
            public string Email { get; set; } = "";
            public DateTime Fecha_Nacimiento { get; set; }
            public string Telefono { get; set; } = "";

            public string Provincia { get; set; } = "";
            public string Canton { get; set; } = "";
            public string Distrito { get; set; } = "";
        }
    }
}
