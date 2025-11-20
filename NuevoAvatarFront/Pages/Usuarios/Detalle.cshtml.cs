using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Usuarios
{
    public class DetalleModel : PageModel
    {
        private readonly IUsuarioApiClient _usuarioApi;
        private readonly IBitacoraApiClient _bitacoraApi;

        public DetalleModel(IUsuarioApiClient usuarioApi, IBitacoraApiClient bitacoraApi)
        {
            _usuarioApi = usuarioApi;
            _bitacoraApi = bitacoraApi;
        }

        public UsuarioDto? Usuario { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            var (ok, status, data, message) = await _usuarioApi.GetByIdAsync(id, idUsuarioAccion);

            if (!ok || data == null)
            {
                ModelState.AddModelError(string.Empty, message ?? "Usuario no encontrado.");
                return Page();
            }

            Usuario = data;

            await _bitacoraApi.RegistrarAsync(idUsuarioAccion, $"Consulta detalle usuario {id}");
            return Page();
        }
    }
}
