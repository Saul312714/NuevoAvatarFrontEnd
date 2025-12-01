using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.HU_MICHEL.Perfil
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

        public UsuarioDto? Usuario { get; set; }

        [BindProperty]
        public string? NuevaContrasena { get; set; }

        [BindProperty]
        public string? ConfirmarContrasena { get; set; }

        public string? MensajeError { get; set; }
        public string? MensajeOk { get; set; }

        // ===========================
        // GET: cargar datos de perfil
        // ===========================
        public async Task<IActionResult> OnGetAsync()
        {
            var idUsuario = HttpContext.Session.GetInt32("UsuarioID");

            if (idUsuario is null || idUsuario == 0)
            {
                // Si no hay usuario en sesión, mandar al login
                return RedirectToPage("/HU_MICHEL/Login/Index");
            }

            var (ok, _, data, msg) = await _usuarioApi.GetByIdAsync(idUsuario.Value, idUsuario.Value);

            if (!ok || data is null)
            {
                MensajeError = msg ?? "No se pudo cargar el perfil del usuario.";
                return Page();
            }

            Usuario = data;

            // 🔹 Bitácora: consulta de pantalla Mi Perfil
            try
            {
                await _bitacoraApi.RegistrarAsync(idUsuario.Value, "Ingreso a la pantalla Mi perfil");
            }
            catch
            {
                // No romper la página si falla la bitácora
            }

            return Page();
        }

        // ==================================
        // POST: cambiar la contraseña propia
        // ==================================
        public async Task<IActionResult> OnPostAsync()
        {
            var idUsuario = HttpContext.Session.GetInt32("UsuarioID");

            if (idUsuario is null || idUsuario == 0)
            {
                return RedirectToPage("/HU_MICHEL/Login/Index");
            }

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(NuevaContrasena) ||
                string.IsNullOrWhiteSpace(ConfirmarContrasena))
            {
                MensajeError = "Debes ingresar la nueva contraseña y su confirmación.";
                await CargarUsuarioAsync(idUsuario.Value);
                return Page();
            }

            if (NuevaContrasena != ConfirmarContrasena)
            {
                MensajeError = "Las contraseñas no coinciden.";
                await CargarUsuarioAsync(idUsuario.Value);
                return Page();
            }

            // 🔹 Máximo 100 caracteres
            if (NuevaContrasena.Length > 100)
            {
                MensajeError = "La contraseña no puede tener más de 100 caracteres.";
                await CargarUsuarioAsync(idUsuario.Value);
                return Page();
            }

            // Traer el usuario actual desde el API
            var (okUser, _, dataUser, msgUser) = await _usuarioApi.GetByIdAsync(idUsuario.Value, idUsuario.Value);

            if (!okUser || dataUser is null)
            {
                MensajeError = msgUser ?? "No se pudo cargar la información del usuario.";
                return Page();
            }

            // Solo cambiamos la contraseña en el DTO y reutilizamos el mismo UPDATE
            dataUser.Contrasena = NuevaContrasena;

            var (okUpdate, _, _, msgUpdate) = await _usuarioApi.UpdateAsync(dataUser, idUsuario.Value);

            if (!okUpdate)
            {
                MensajeError = msgUpdate ?? "No se pudo actualizar la contraseña.";
                Usuario = dataUser;
                return Page();
            }

            // 🔹 Bitácora: cambio de contraseña
            try
            {
                await _bitacoraApi.RegistrarAsync(idUsuario.Value, "Cambio de contraseña desde Mi perfil");
            }
            catch
            {
                // No interrumpir el cierre de sesión si falla la bitácora
            }

            // ============================
            // ÉXITO: limpiar sesión y cookies
            // ============================

            // 1. Limpiar sesión
            HttpContext.Session.Clear();

            // 2. Borrar TODAS las cookies (igual que en tu logout)
            foreach (var cookie in Request.Cookies.Keys)
                Response.Cookies.Delete(cookie);

            // 3. Redirigir al login con indicador de logout
            return Redirect("/HU_MICHEL/Login/Index?logout=1");
        }

        // Método auxiliar para recargar el usuario cuando hay errores
        private async Task CargarUsuarioAsync(int idUsuario)
        {
            var (ok, _, data, _) = await _usuarioApi.GetByIdAsync(idUsuario, idUsuario);
            if (ok)
                Usuario = data;
        }
    }
}
