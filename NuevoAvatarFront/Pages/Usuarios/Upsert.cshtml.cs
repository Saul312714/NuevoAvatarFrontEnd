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
                var (ok, status, data, message) = await _usuarioApi.GetByIdAsync(id.Value, idUsuarioAccion);

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
            // VALIDACIONES DEL FRONTEND
            // ============================
            if (string.IsNullOrWhiteSpace(Usuario.Nombre))
            {
                ModelState.AddModelError(string.Empty, "El nombre es requerido.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Usuario.Email) ||
                !Regex.IsMatch(Usuario.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError(string.Empty, "Formato de correo inválido.");
                return Page();
            }

            if (Usuario.Email.EndsWith("@cuc.cr"))
            {
                Usuario.ID_Rol = 1; // estudiante
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

            // Para nuevos usuarios -> contraseña obligatoria
            if (Usuario.ID_Usuario == 0 && string.IsNullOrWhiteSpace(Usuario.Contrasena))
            {
                ModelState.AddModelError(string.Empty, "La contraseña es obligatoria.");
                return Page();
            }

            // ============================
            // CREAR
            // ============================
            if (!EsEdicion)
            {
                var (ok, status, data, message) = await _usuarioApi.CreateAsync(Usuario, idUsuarioAccion);

                if (!ok)
                {
                    ModelState.AddModelError(string.Empty, message ?? "Error al crear usuario.");
                    return Page();
                }

                await _bitacoraApi.RegistrarAsync(idUsuarioAccion, $"Alta usuario {Usuario.Email}");
                return RedirectToPage("/Usuarios/Index");
            }

            // ============================
            // ACTUALIZAR
            // ============================
            var (okUpdate, statusUpdate, dataUpdate, msgUpdate)
                = await _usuarioApi.UpdateAsync(Usuario, idUsuarioAccion);

            if (!okUpdate)
            {
                ModelState.AddModelError(string.Empty, msgUpdate ?? "Error al actualizar.");
                return Page();
            }

            await _bitacoraApi.RegistrarAsync(idUsuarioAccion, $"Actualización usuario {Usuario.Email}");

            return RedirectToPage("/Usuarios/Index");
        }
    }
}
