using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NuevoAvatarFront.Pages.Expedientes
{
    public class UpsertModel : PageModel
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public UpsertModel(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient();
            _config = config;
        }

        public string? ErrorMessage { get; set; }

        [BindProperty]
        public ExpedienteDto Expediente { get; set; } = new();

        // Estas propiedades son enlazadas desde el formulario
        [BindProperty] public string Provincia { get; set; } = "";
        [BindProperty] public string Canton { get; set; } = "";
        [BindProperty] public string Distrito { get; set; } = "";
        [BindProperty] public string OtrasSenas { get; set; } = "";

        public bool EsEdicion => Expediente.ID_Estudiante > 0;

        public List<ProvinciaDto> Provincias { get; set; } = new();
        public List<CantonDto> Cantones { get; set; } = new();
        public List<DistritoDto> Distritos { get; set; } = new();

        // ============================================================
        // GET
        // ============================================================
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            CargarCatalogos();

            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (string.IsNullOrWhiteSpace(token) || userId == null)
                return Redirect("/HU_MICHEL/Login");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            if (id.HasValue && id.Value > 0)
            {
                var url = $"{_config["ApiMatricula"]}/expediente/{id}?idUsuario={userId}";
                var dto = await _http.GetFromJsonAsync<ExpedienteDto>(url);

                if (dto != null)
                {
                    Expediente = dto;

                    // Descomponer dirección si es válida
                    if (!string.IsNullOrWhiteSpace(dto.Direccion))
                    {
                        var partes = dto.Direccion.Split(" - ");
                        if (partes.Length >= 3)
                        {
                            Provincia = partes[0];
                            Canton = partes[1];
                            Distrito = partes[2];

                            // Si hay más partes, son otras señas
                            if (partes.Length > 3)
                            {
                                OtrasSenas = string.Join(" - ", partes.Skip(3));
                            }
                        }
                    }
                }
            }

            return Page();
        }

        // ============================================================
        // POST
        // ============================================================
        public async Task<IActionResult> OnPostAsync()
        {
            CargarCatalogos();

            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (string.IsNullOrWhiteSpace(token) || userId == null)
                return Redirect("/HU_MICHEL/Login");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Validación dominio correo
            if (!Expediente.Email.EndsWith("@cuc.cr") &&
                !Expediente.Email.EndsWith("@cuc.ac.cr"))
            {
                ErrorMessage = "El email debe pertenecer al dominio cuc.cr o cuc.ac.cr.";
                return Page();
            }

            // Construcción de dirección total
            if (!string.IsNullOrWhiteSpace(OtrasSenas))
            {
                Expediente.Direccion = $"{Provincia} - {Canton} - {Distrito} - {OtrasSenas}";
            }
            else
            {
                Expediente.Direccion = $"{Provincia} - {Canton} - {Distrito}";
            }

            HttpResponseMessage response;

            if (!EsEdicion)
            {
                // Crear
                response = await _http.PostAsJsonAsync(
                    $"{_config["ApiMatricula"]}/expediente?idUsuario={userId}",
                    Expediente
                );
            }
            else
            {
                // Editar - ID en la URL
                response = await _http.PutAsJsonAsync(
                    $"{_config["ApiMatricula"]}/expediente/{Expediente.ID_Estudiante}?idUsuario={userId}",
                    Expediente
                );
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Error al guardar: {response.StatusCode} - {errorContent}";
                return Page();
            }

            return RedirectToPage("./Index");
        }

        // ============================================================
        // Cargar catálogos
        // ============================================================
        private void CargarCatalogos()
        {
            Provincias = MockDirecciones.Provincias.Take(7).ToList();
            Cantones = MockDirecciones.Cantones.Take(100).ToList();
            Distritos = MockDirecciones.Distritos.Take(100).ToList();
        }

        // ============================================================
        // DTOs
        // ============================================================
        public class ExpedienteDto
        {
            public int ID_Estudiante { get; set; }
            public string Identificacion { get; set; } = "";
            public string Tipo_Identificacion { get; set; } = "";
            public string Nombre { get; set; } = "";
            public string Email { get; set; } = "";
            public DateTime Fecha_Nacimiento { get; set; }
            public string Direccion { get; set; } = "";
            public string Telefono { get; set; } = "";
        }

        public class ProvinciaDto
        {
            public string Codigo { get; set; } = "";
            public string Nombre { get; set; } = "";
        }

        public class CantonDto
        {
            public string provincia { get; set; } = "";
            public string codigo { get; set; } = "";
            public string nombre { get; set; } = "";
        }

        public class DistritoDto
        {
            public string canton { get; set; } = "";
            public string codigo { get; set; } = "";
            public string nombre { get; set; } = "";
        }
    }
}