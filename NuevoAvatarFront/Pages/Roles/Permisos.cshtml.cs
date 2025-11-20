using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Roles
{
    public class PermisosModel : PageModel
    {
        private readonly IRolModuloApiClient _rolModuloApi;
        private readonly IModuloApiClient _moduloApi;

        public PermisosModel(IRolModuloApiClient rolModuloApi, IModuloApiClient moduloApi)
        {
            _rolModuloApi = rolModuloApi;
            _moduloApi = moduloApi;
        }

        [BindProperty]
        public int ID_Rol { get; set; }

        public string NombreRol { get; set; } = string.Empty;

        [BindProperty]
        public List<RolModuloDto> Permisos { get; set; } = new();

        public List<ModuloDto> Modulos { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int idRol, string? nombreRol)
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return Redirect("/HU_MICHEL/Login");

            ID_Rol = idRol;
            NombreRol = nombreRol ?? "Rol";

            // 1. Obtener módulos disponibles
            Modulos = await _moduloApi.GetAllAsync();

            // 2. Obtener permisos actuales
            var permisosActuales = await _rolModuloApi.GetByRolAsync(idRol);

            // 3. Combinar lista de módulos con permisos
            Permisos = Modulos
                .Select(m =>
                {
                    var permiso = permisosActuales.FirstOrDefault(p => p.ID_Modulo == m.ID_Modulo);

                    return new RolModuloDto
                    {
                        ID_Rol = idRol,
                        ID_Modulo = m.ID_Modulo,
                        PuedeVer = permiso?.PuedeVer ?? false
                    };
                })
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return Redirect("/HU_MICHEL/Login");

            var ok = await _rolModuloApi.UpdatePermisosAsync(ID_Rol, Permisos);

            if (ok)
                TempData["success"] = "Permisos guardados correctamente.";
            else
                TempData["error"] = "Error al guardar permisos.";

            return RedirectToPage("/Roles/Permisos", new { idRol = ID_Rol });
        }
    }
}
