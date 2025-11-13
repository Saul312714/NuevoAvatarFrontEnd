using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace NuevoAvatarFront.Pages.Parametros
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

        public List<ParametroDto> Parametros { get; set; } = new();

        public async Task OnGetAsync()
        {
            var token = HttpContext.Session.GetString("access_token") ?? "";
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var url = $"{_config["ApiUsuariosRoles"]}/parametro";
            var result = await _http.GetFromJsonAsync<List<ParametroDto>>(url);
            Parametros = result ?? new();
        }

     
        public async Task<IActionResult> OnPostSaveAsync(ParametroDto data)
        {
            var token = HttpContext.Session.GetString("access_token") ?? "";
            var idUsuario = HttpContext.Session.GetInt32("usuarioID") ?? 1;
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var baseUrl = $"{_config["ApiUsuariosRoles"]}/parametro";
            HttpResponseMessage response;

            
            if (!string.IsNullOrWhiteSpace(data.ID_Parametro))
                response = await _http.PutAsJsonAsync($"{baseUrl}/{data.ID_Parametro}?idUsuarioAccion={idUsuario}", data);
            else
                response = await _http.PostAsJsonAsync($"{baseUrl}?idUsuarioAccion={idUsuario}", data);

            if (response.IsSuccessStatusCode)
                await RegistrarBitacora(idUsuario, "Gestión de parámetros del sistema");

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var token = HttpContext.Session.GetString("access_token") ?? "";
            var idUsuario = HttpContext.Session.GetInt32("usuarioID") ?? 1;
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var url = $"{_config["ApiUsuariosRoles"]}/parametro/{id}?idUsuarioAccion={idUsuario}";
            await _http.DeleteAsync(url);
            await RegistrarBitacora(idUsuario, $"Eliminó parámetro {id}");
            return RedirectToPage();
        }

        private async Task RegistrarBitacora(int idUsuario, string accion)
        {
            var url = $"{_config["ApiSeguridad"]}/bitacora";
            await _http.PostAsJsonAsync(url, new { ID_Usuario = idUsuario, Fecha = DateTime.Now, Accion = accion });
        }

        public class ParametroDto
        {
            public string ID_Parametro { get; set; } = "";
            public string Valor { get; set; } = "";
        }
    }
}
