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
                var u = await _usuarioApi.GetByIdAsync(id.Value, idUsuarioAccion);
                if (u == null)
                    return NotFound();

                Usuario = u;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            // --- VALIDACIONES ---
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

            // Validación de dominio y asignación correcta según tu tabla
            if (Usuario.Email.EndsWith("@cuc.cr"))
            {
                Usuario.ID_Rol = 1; // Estudiante
            }
            else if (Usuario.Email.EndsWith("@cuc.ac.cr"))
            {
                // Solo Profesor o Administrador
                if (Usuario.ID_Rol != 2 && Usuario.ID_Rol != 7)
                {
                    ModelState.AddModelError(string.Empty,
                        "El dominio cuc.ac.cr solo es válido para Profesor o Administrador.");
                    return Page();
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty,
                    "Dominio no permitido. Solo cuc.cr o cuc.ac.cr.");
                return Page();
            }

            // Si no hay contraseña
            Usuario.Contrasena ??= string.Empty;

            // --- CREAR ---
            if (Usuario.ID_Usuario == 0)
            {
                if (string.IsNullOrWhiteSpace(Usuario.Contrasena))
                {
                    ModelState.AddModelError(string.Empty, "La contraseña es obligatoria para nuevos usuarios.");
                    return Page();
                }

                var creado = await _usuarioApi.CreateAsync(Usuario, idUsuarioAccion);
                if (creado == null)
                {
                    ModelState.AddModelError(string.Empty, "Error al crear el usuario.");
                    return Page();
                }

                await _bitacoraApi.RegistrarAsync(idUsuarioAccion, $"Alta de usuario {Usuario.Email}");
            }
            // --- EDITAR ---
            else
            {
                var actualizado = await _usuarioApi.UpdateAsync(Usuario, idUsuarioAccion);
                if (actualizado == null)
                {
                    ModelState.AddModelError(string.Empty, "Error al actualizar el usuario.");
                    return Page();
                }

                await _bitacoraApi.RegistrarAsync(idUsuarioAccion, $"Modificación de usuario {Usuario.Email}");
            }

            return RedirectToPage("/Usuarios/Index");
        }
    }
}
