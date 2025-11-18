using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Usuarios
{
    public class IndexModel : PageModel
    {
        private readonly IUsuarioApiClient _usuarioApi;
        private readonly IBitacoraApiClient _bitacoraApi;

        public IndexModel(IUsuarioApiClient usuarioApi, IBitacoraApiClient bitacoraApi)
        {
            _usuarioApi = usuarioApi;
            _bitacoraApi = bitacoraApi;
        }

        public List<UsuarioDto> Usuarios { get; set; } = new();
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string? Filtro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Rol { get; set; }

        public async Task OnGetAsync(int page = 1)
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            Page = page;

            var (ok, status, data, message) = await _usuarioApi.GetAllAsync(Filtro, Rol, page, PageSize);

            if (!ok)
                ModelState.AddModelError(string.Empty, message ?? "Error al obtener usuarios.");
            else
                Usuarios = data ?? new();

            // total sin paginar
            var (okTotal, statusTotal, dataTotal, msgTotal) = await _usuarioApi.GetAllAsync(Filtro, Rol);

            if (okTotal && dataTotal != null)
                TotalPages = (int)Math.Ceiling(dataTotal.Count / (double)PageSize);

            await _bitacoraApi.RegistrarAsync(idUsuarioAccion, "Consulta listado usuarios");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            var (ok, status, message) = await _usuarioApi.DeleteAsync(id, idUsuarioAccion);

            if (!ok)
            {
                TempData["Error"] = message;
                return RedirectToPage();
            }

            await _bitacoraApi.RegistrarAsync(idUsuarioAccion, $"Eliminación usuario {id}");

            return RedirectToPage();
        }
    }
}
