using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

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
        }

        [BindProperty]
        public RolDto Rol { get; set; } = new();

        public bool EsEdicion => Rol.ID_Rol > 0;
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            if (id.HasValue)
            {
                var (ok, status, data, msg) = await _rolApi.GetByIdAsync(id.Value, idUsuarioAccion);

                if (!ok || data == null)
                {
                    ErrorMessage = msg ?? "Rol no encontrado.";
                    return Page();
                }

                Rol = data;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            Rol.Nombre = Rol.Nombre?.Trim();

            if (string.IsNullOrWhiteSpace(Rol.Nombre))
            {
                ModelState.AddModelError(nameof(Rol.Nombre), "El nombre del rol es obligatorio.");
                return Page();
            }

            if (!ModelState.IsValid)
                return Page();

            // CREAR
            if (!EsEdicion)
            {
                var (ok, status, data, msg) = await _rolApi.CreateAsync(Rol, idUsuarioAccion);

                if (!ok)
                {
                    ModelState.AddModelError(string.Empty, msg ?? "Error al crear el rol.");
                    return Page();
                }

                await _bitacoraApi.RegistrarAsync(idUsuarioAccion,
                    $"Creación de rol: {Rol.Nombre}");

                TempData["SuccessMessage"] = "Rol creado correctamente.";
                return RedirectToPage("/Roles/Index");
            }

            // ACTUALIZAR
            var (okUpdate, statusUpdate, dataUpdate, msgUpdate) =
                await _rolApi.UpdateAsync(Rol, idUsuarioAccion);

            if (!okUpdate)
            {
                ModelState.AddModelError(string.Empty, msgUpdate ?? "Error al actualizar el rol.");
                return Page();
            }

            await _bitacoraApi.RegistrarAsync(idUsuarioAccion,
                $"Modificación de rol: {Rol.Nombre}");

            TempData["SuccessMessage"] = "Rol actualizado correctamente.";
            return RedirectToPage("/Roles/Index");
        }
    }
}
