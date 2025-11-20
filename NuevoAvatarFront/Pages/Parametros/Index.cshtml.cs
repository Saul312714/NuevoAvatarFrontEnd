using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
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

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalPages { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            // Leer página desde querystring
            if (Request.Query.ContainsKey("page"))
            {
                int.TryParse(Request.Query["page"], out int p);
                Page = p > 0 ? p : 1;
            }

            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (string.IsNullOrWhiteSpace(token) || userId == null)
            {
                Response.Redirect("/HU_MICHEL/Login");
                return;
            }

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = $"{_config["ApiUsuariosRoles"]}/parametro?idUsuarioAccion={userId}";
            var json = await _http.GetStringAsync(url);

            var lista = JsonSerializer.Deserialize<List<ParametroDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new();

            TotalPages = (int)Math.Ceiling(lista.Count / (double)PageSize);

            if (Page > TotalPages && TotalPages > 0)
                Page = TotalPages;

            Parametros = lista
                .Skip((Page - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        // ================== DELETE (handler Delete) ==================
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (string.IsNullOrWhiteSpace(token) || userId == null)
                return Redirect("/HU_MICHEL/Login");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _http.DeleteAsync(
                $"{_config["ApiUsuariosRoles"]}/parametro/{id}?idUsuarioAccion={userId}");

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "No se pudo eliminar el parámetro.";
                await OnGetAsync();
                return Page();
            }

            // Bitácora (no bloqueante)
            try
            {
                await _http.PostAsJsonAsync($"{_config["ApiSeguridad"]}/bitacora", new
                {
                    ID_Usuario = userId,
                    Fecha = DateTime.Now,
                    Accion = $"Eliminó parámetro {id}"
                });
            }
            catch { }

            // mantener página actual
            int currentPage = 1;
            if (Request.Query.ContainsKey("page"))
                int.TryParse(Request.Query["page"], out currentPage);

            return RedirectToPage("./Index", new { page = currentPage });
        }

        // DTO
        public class ParametroDto
        {
            public string ID_Parametro { get; set; } = "";
            public string Valor { get; set; } = "";
        }
    }
}
