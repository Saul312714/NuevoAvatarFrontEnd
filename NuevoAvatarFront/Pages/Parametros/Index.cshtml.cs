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

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalPages { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            if (Request.Query.ContainsKey("page"))
            {
                int.TryParse(Request.Query["page"], out int p);
                Page = p > 0 ? p : 1;
            }

            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (token is null || userId is null)
            {
                Response.Redirect("/HU_MICHEL/Login");
                return;
            }

            var lista = await _parametroApi.GetAllAsync(userId.Value, token);

            TotalPages = (int)Math.Ceiling(lista.Count / (double)PageSize);
            if (Page > TotalPages && TotalPages > 0)
                Page = TotalPages;

            Parametros = lista
                .Skip((Page - 1) * PageSize)
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

            int currentPage = 1;
            if (Request.Query.ContainsKey("page"))
                int.TryParse(Request.Query["page"], out currentPage);

            return RedirectToPage("./Index", new { page = currentPage });
        }
    }
}
