using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Roles
{
    public class IndexModel : PageModel
    {
        private readonly IRolApiClient _rolApi;
        private readonly IBitacoraApiClient _bitacoraApi;

        public IndexModel(IRolApiClient rolApi, IBitacoraApiClient bitacoraApi)
        {
            _rolApi = rolApi;
            _bitacoraApi = bitacoraApi;
        }

        public List<RolDto> Roles { get; set; } = new();

        public async Task OnGetAsync()
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;
            Roles = await _rolApi.GetAllRolesAsync(idUsuarioAccion);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            // Obtener rol antes de eliminar (para registrar en bitácora)
            var rolAntes = await _rolApi.GetByIdAsync(id, idUsuarioAccion);

            var ok = await _rolApi.DeleteAsync(id, idUsuarioAccion);

            if (!ok)
            {
                ModelState.AddModelError("", "Error al eliminar el rol.");
                return Page();
            }

            // Registrar en bitácora
            await _bitacoraApi.RegistrarAsync(idUsuarioAccion,
                $"Eliminación de rol: {rolAntes?.Nombre}");

            return RedirectToPage();
        }
    }

}
