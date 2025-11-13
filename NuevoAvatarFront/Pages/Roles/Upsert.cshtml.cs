using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;
using System.Text.RegularExpressions;

namespace NuevoAvatarFront.Pages.Roles
{
    public class UpsertModel : PageModel
    {
        private readonly IRolApiClient _rolApi;
        private readonly IBitacoraApiClient _bitacoraApi;

        public UpsertModel(IRolApiClient rolApi, IBitacoraApiClient bitacoraApi)
        {
            _rolApi = rolApi;
            _bitacoraApi = bitacoraApi;
            Rol = new RolDto();
        }

        [BindProperty] public RolDto Rol { get; set; }

        public bool EsEdicion => Rol.ID_Rol > 0;

        public RolDto RolAntes { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue)
            {
                Rol = await _rolApi.GetByIdAsync(id.Value, HttpContext.Session.GetInt32("UsuarioID") ?? 0);
                RolAntes = Rol;
                if (Rol == null) return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            // Normalizar
            Rol.Nombre = Rol.Nombre?.Trim();

            if (!ModelState.IsValid)
                return Page();

            if (EsEdicion)
            {
                // Obtener datos ANTES de actualizar
                var antes = await _rolApi.GetByIdAsync(Rol.ID_Rol, idUsuarioAccion);

                // Actualizar
                await _rolApi.UpdateAsync(Rol, idUsuarioAccion);

                // Calcular JSON DIFF
                var diff = new
                {
                    Antes = antes,
                    Despues = Rol
                };

                await _bitacoraApi.RegistrarAsync(idUsuarioAccion,
                    $"Modificación de rol: {Rol.Nombre} | Cambios: {System.Text.Json.JsonSerializer.Serialize(diff)}");
            }
            else
            {
                await _rolApi.CreateAsync(Rol, idUsuarioAccion);

                // Registrar en bitácora
                await _bitacoraApi.RegistrarAsync(idUsuarioAccion,
                    $"Creación de rol: {Rol.Nombre}");
            }

            return RedirectToPage("/Roles/Index");
        }
    }
}
