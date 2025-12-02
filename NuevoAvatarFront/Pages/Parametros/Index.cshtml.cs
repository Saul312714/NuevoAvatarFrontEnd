using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Parametros
{
    public class IndexModel : PageModel
    {
        private readonly IParametroApiClient _parametroApi;

        public IndexModel(IParametroApiClient parametroApi)
        {
            _parametroApi = parametroApi;
        }

        public List<ParametroDto> Parametros { get; set; } = new();

        public int PageSize { get; set; } = 5;
        public int TotalPages { get; set; }

        // 🔹 EL NUEVO NOMBRE (NO USAR "Page")
        public int CurrentPage { get; set; } = 1;

        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            // Leer parámetro "page" correctamente
            if (Request.Query.ContainsKey("page"))
            {
                int.TryParse(Request.Query["page"], out int p);
                CurrentPage = p > 0 ? p : 1;
            }

            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (token is null || userId is null)
            {
                Response.Redirect("/HU_MICHEL/Login");
                return;
            }

            var lista = await _parametroApi.GetAllAsync(userId.Value, token);

            // Total de páginas
            TotalPages = (int)Math.Ceiling(lista.Count / (double)PageSize);
            if (CurrentPage > TotalPages && TotalPages > 0)
                CurrentPage = TotalPages;

            // Aplicar paginación
            Parametros = lista
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (token is null || userId is null)
                return Redirect("/HU_MICHEL/Login");

            var result = await _parametroApi.DeleteAsync(id, userId.Value, token);

            if (!result.ok)
            {
                ErrorMessage = "No se pudo eliminar el parámetro.";
                await OnGetAsync();
                return Page();
            }

            // Mantener la página actual después de eliminar
            int currentPage = 1;
            if (Request.Query.ContainsKey("page"))
                int.TryParse(Request.Query["page"], out currentPage);

            return RedirectToPage("./Index", new { page = currentPage });
        }
    }
}
