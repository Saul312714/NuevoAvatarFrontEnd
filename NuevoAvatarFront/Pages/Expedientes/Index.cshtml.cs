using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

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

        public List<ExpedienteDto> Expedientes { get; set; } = new();

        public async Task OnGetAsync()
        {
            var token = HttpContext.Session.GetString("access_token") ?? "";
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var url = $"{_config["ApiMatricula"]}/expediente";
            var result = await _http.GetFromJsonAsync<List<ExpedienteDto>>(url);
            Expedientes = result ?? new();
        }

        public async Task<IActionResult> OnPostSaveAsync(ExpedienteDto data)
        {
            var token = HttpContext.Session.GetString("access_token") ?? "";
            var idUsuario = HttpContext.Session.GetInt32("usuarioID") ?? 1;
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var url = $"{_config["ApiMatricula"]}/expediente";
            await _http.PostAsJsonAsync(url, data);
            await RegistrarBitacora(idUsuario, "Gestión de expedientes");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var token = HttpContext.Session.GetString("access_token") ?? "";
            var idUsuario = HttpContext.Session.GetInt32("usuarioID") ?? 1;
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var url = $"{_config["ApiMatricula"]}/expediente/{id}";
            await _http.DeleteAsync(url);
            await RegistrarBitacora(idUsuario, $"Eliminó expediente {id}");
            return RedirectToPage();
        }

        private async Task RegistrarBitacora(int idUsuario, string accion)
        {
            var url = $"{_config["ApiSeguridad"]}/bitacora";
            await _http.PostAsJsonAsync(url, new { ID_Usuario = idUsuario, Fecha = DateTime.Now, Accion = accion });
        }

        public class ExpedienteDto
        {
            public string Identificacion { get; set; } = "";
            public string Nombre { get; set; } = "";
            public string Email { get; set; } = "";
            public string Telefonos { get; set; } = "";
            public string Provincia { get; set; } = "";
            public string Canton { get; set; } = "";
            public string Distrito { get; set; } = "";
        }
    }

}
