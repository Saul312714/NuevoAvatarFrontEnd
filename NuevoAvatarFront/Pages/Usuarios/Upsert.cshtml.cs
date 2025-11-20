using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;
using System.Text.RegularExpressions;

namespace NuevoAvatarFront.Pages.Usuarios
{
    public class UpsertModel : PageModel
    {
        private readonly IUsuarioApiClient _usuarioApi;
        private readonly IBitacoraApiClient _bitacoraApi;

        public UpsertModel(IUsuarioApiClient usuarioApi, IBitacoraApiClient bitacoraApi)
        {
            _usuarioApi = usuarioApi;
            _bitacoraApi = bitacoraApi;
        }

        [BindProperty]
        public UsuarioDto Usuario { get; set; } = new();

        public bool EsEdicion => Usuario.ID_Usuario > 0;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            if (id.HasValue)
            {
                var (ok, status, data, message) =
                    await _usuarioApi.GetByIdAsync(id.Value, idUsuarioAccion);

                if (!ok || data == null)
                {
                    ModelState.AddModelError(string.Empty, message ?? "Usuario no encontrado.");
                    return Page();
                }

                Usuario = data;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            // ============================
            // VALIDACIONES DEL NOMBRE
            // ============================
            if (string.IsNullOrWhiteSpace(Usuario.Nombre))
            {
                ModelState.AddModelError(string.Empty, "El nombre es requerido.");
                return Page();
            }

            // Solo letras y espacios (incluye tildes y Ò)
            if (!Regex.IsMatch(Usuario.Nombre, @"^[A-Za-z¡…Õ”⁄·ÈÌÛ˙—Ò ]+$"))
            {
                ModelState.AddModelError(string.Empty,
                    "El nombre solo puede contener letras y espacios.");
                return Page();
            }

            // ============================
            // VALIDACI”N EMAIL
            // ============================
            if (string.IsNullOrWhiteSpace(Usuario.Email) ||
                !Regex.IsMatch(Usuario.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError(string.Empty, "Formato de correo inv·lido.");
                return Page();
            }

            // ============================
            // VALIDACI”N DOMINIOS
            // ============================
            if (Usuario.Email.EndsWith("@cuc.cr"))
            {
                Usuario.ID_Rol = 3; // estudiante
            }
            else if (Usuario.Email.EndsWith("@cuc.ac.cr"))
            {
                if (Usuario.ID_Rol != 2 && Usuario.ID_Rol != 7)
                {
                    ModelState.AddModelError(string.Empty,
                        "Solo Profesor o Administrador pueden usar cuc.ac.cr");
                    return Page();
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Dominio no permitido.");
                return Page();
            }

            // ============================
            // CONTRASE—A (solo al crear)
            // ============================
            if (!EsEdicion && string.IsNullOrWhiteSpace(Usuario.Contrasena))
            {
                ModelState.AddModelError(string.Empty, "La contraseÒa es obligatoria.");
                return Page();
            }

            // ============================
            // CREAR
            // ============================
            if (!EsEdicion)
            {
                var (ok, status, data, message) =
                    await _usuarioApi.CreateAsync(Usuario, idUsuarioAccion);

                if (!ok)
                {
                    ModelState.AddModelError(string.Empty, message ?? "Error al crear usuario.");
                    return Page();
                }

                await _bitacoraApi.RegistrarAsync(idUsuarioAccion,
                    $"Alta usuario {Usuario.Email}");

                return RedirectToPage("/Usuarios/Index");
            }

            // ============================
            // ACTUALIZAR
            // ============================
            var (okUpdate, statusUpdate, dataUpdate, msgUpdate) =
                await _usuarioApi.UpdateAsync(Usuario, idUsuarioAccion);

            if (!okUpdate)
            {
                ModelState.AddModelError(string.Empty, msgUpdate ?? "Error al actualizar.");
                return Page();
            }

            await _bitacoraApi.RegistrarAsync(idUsuarioAccion,
                $"ActualizaciÛn usuario {Usuario.Email}");

            return RedirectToPage("/Usuarios/Index");
        }
    }
}
