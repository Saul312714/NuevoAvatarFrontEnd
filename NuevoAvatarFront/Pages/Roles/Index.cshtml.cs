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
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            var (ok, status, data, msg) = await _rolApi.GetAllAsync(idUsuarioAccion);

            if (!ok)
            {
                ErrorMessage = msg ?? "No fue posible obtener la lista de roles.";
                Roles = new();
                return;
            }

            Roles = data ?? new();

            await _bitacoraApi.RegistrarAsync(idUsuarioAccion, "Consulta listado de roles");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            var (ok, status, msg) = await _rolApi.DeleteAsync(id, idUsuarioAccion);

            if (!ok)
            {
                TempData["ErrorMessage"] = msg ?? "No se pudo eliminar el rol.";
                return RedirectToPage();
            }

            await _bitacoraApi.RegistrarAsync(idUsuarioAccion, $"Eliminación de rol {id}");
            TempData["SuccessMessage"] = "Rol eliminado correctamente.";

            return RedirectToPage();
        }
    }
}
