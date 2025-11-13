using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NuevoAvatarFront.Pages.Parametros
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

        [BindProperty]
        public ParametroDto Parametro { get; set; } = new();

        // este valor viene del {id?} de la ruta
        [BindProperty(SupportsGet = true)]
        public string? Id { get; set; }

        public bool EsEdicion => !string.IsNullOrWhiteSpace(Id);

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (string.IsNullOrWhiteSpace(token) || userId == null)
                return Redirect("/HU_MICHEL/Login");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Si viene ID → cargar parámetro
            if (EsEdicion)
            {
                var url = $"{_config["ApiUsuariosRoles"]}/parametro/{Id}?idUsuarioAccion={userId}";
                Parametro = await _http.GetFromJsonAsync<ParametroDto>(url) ?? new();
            }

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (string.IsNullOrWhiteSpace(token) || userId == null)
                return Redirect("/HU_MICHEL/Login");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Validaciones
            if (Parametro.ID_Parametro.Length > 10 ||
                Parametro.ID_Parametro != Parametro.ID_Parametro.ToUpper())
            {
                ErrorMessage = "El ID debe estar en MAYÚSCULAS y tener máximo 10 caracteres.";
                return Page();
            }

            if (Parametro.Valor.Length > 500)
            {
                ErrorMessage = "El valor no puede superar 500 caracteres.";
                return Page();
            }

            HttpResponseMessage response;

            // POST → Crear
            if (!EsEdicion)
            {
                response = await _http.PostAsJsonAsync(
                    $"{_config["ApiUsuariosRoles"]}/parametro?idUsuarioAccion={userId}",
                    Parametro
                );
            }
            else
            {
                // PUT → Editar
                response = await _http.PutAsJsonAsync(
                    $"{_config["ApiUsuariosRoles"]}/parametro/{Parametro.ID_Parametro}?idUsuarioAccion={userId}",
                    Parametro
                );
            }

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "No se pudo guardar el parámetro.";
                return Page(); // ← muestra error pero NO recarga
            }

            return RedirectToPage("./Index"); // ← ahora sí vuelve a la tabla siempre
        }

        public class ParametroDto
        {
            public string ID_Parametro { get; set; } = "";
            public string Valor { get; set; } = "";
        }
    }
}
