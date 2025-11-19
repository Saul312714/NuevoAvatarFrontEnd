using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Usuarios
{
    public class IndexModel : PageModel
    {
        private readonly IUsuarioApiClient _usuarioApi;
        private readonly IHttpContextAccessor _context;

        public IndexModel(IUsuarioApiClient usuarioApi, IHttpContextAccessor context)
        {
            _usuarioApi = usuarioApi;
            _context = context;
        }

        public List<UsuarioDto> Usuarios { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public int Pagina { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public string? Filtro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Rol { get; set; }

        public async Task<IActionResult> OnGetAsync(int page = 1)
        {
            Pagina = page;

            var idUsuarioAccion = _context.HttpContext?.Session.GetInt32("UsuarioID") ?? 0;

            var result = await _usuarioApi.GetAllAsync(Filtro, Rol, Pagina, 10);

            if (!result.ok)
            {
                ErrorMessage = result.Message ?? "Error cargando usuarios.";
                return Page();
            }

            Usuarios = result.Data ?? new List<UsuarioDto>();

            // El backend ya controla el tamaño, pero generamos paginación client-side:
            TotalPaginas = Usuarios.Count < 10 ? Pagina : Pagina + 1;

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var idUsuarioAccion = _context.HttpContext?.Session.GetInt32("UsuarioID") ?? 0;

            var result = await _usuarioApi.DeleteAsync(id, idUsuarioAccion);

            if (!result.ok)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = "Usuario eliminado correctamente.";
            }

            return RedirectToPage();
        }
    }
}
