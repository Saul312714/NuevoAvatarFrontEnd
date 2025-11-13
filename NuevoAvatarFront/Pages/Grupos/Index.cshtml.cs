using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace NuevoAvatarFront.Pages.Grupos
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

        public List<GrupoDto> Grupos { get; set; } = new();

        public async Task OnGetAsync()
        {
            var token = HttpContext.Session.GetString("access_token") ?? "";
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var url = $"{_config["ApiOfertaAcademica"]}/grupo";
            var result = await _http.GetFromJsonAsync<List<GrupoDto>>(url);
            Grupos = result ?? new();
        }

        public async Task<IActionResult> OnPostSaveAsync(GrupoDto data)
        {
            var token = HttpContext.Session.GetString("access_token") ?? "";
            var idUsuario = HttpContext.Session.GetInt32("usuarioID") ?? 1;
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var url = $"{_config["ApiOfertaAcademica"]}/grupo";
            await _http.PostAsJsonAsync(url, data);
            await RegistrarBitacora(idUsuario, "Gestión de grupos académicos");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var token = HttpContext.Session.GetString("access_token") ?? "";
            var idUsuario = HttpContext.Session.GetInt32("usuarioID") ?? 1;
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var url = $"{_config["ApiOfertaAcademica"]}/grupo/{id}";
            await _http.DeleteAsync(url);
            await RegistrarBitacora(idUsuario, $"Eliminó grupo {id}");
            return RedirectToPage();
        }

        private async Task RegistrarBitacora(int idUsuario, string accion)
        {
            var url = $"{_config["ApiSeguridad"]}/bitacora";
            await _http.PostAsJsonAsync(url, new { ID_Usuario = idUsuario, Fecha = DateTime.Now, Accion = accion });
        }

        public class GrupoDto
        {
            public int Numero { get; set; }
            public string Curso { get; set; } = "";
            public string Profesor { get; set; } = "";
            public string Horario { get; set; } = "";
            public string Periodo { get; set; } = "";
        }
    }

}
