using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Shared
{
    [IgnoreAntiforgeryToken]
    public class _TopbarModel : PageModel
    {
        private readonly IBitacoraApiClient _bitacoraApi;

        public _TopbarModel(IBitacoraApiClient bitacoraApi)
        {
            _bitacoraApi = bitacoraApi;
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            var idUsuario = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            try
            {
                await _bitacoraApi.RegistrarAsync(idUsuario, "Logout de sesión");
            }
            catch
            {
                Console.WriteLine("No se pudo registrar el logout en bitácora.");
            }

            // Limpiar sesión y cookies
            HttpContext.Session.Clear();
            foreach (var cookie in Request.Cookies.Keys)
                Response.Cookies.Delete(cookie);

            // Redirigir al login con mensaje de sesión cerrada
            return Redirect("/HU_MICHEL/Login/Index?logout=1");
        }
    }
}

