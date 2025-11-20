using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;
using System.Text.Json;

namespace NuevoAvatarFront.Pages.Modulos
{
    public class IndexModel : PageModel
    {
        private readonly IModuloApiClient _api;
        private readonly IBitacoraApiClient _bitacora;
        private readonly IRolModuloApiClient _rolModuloApi;
        private readonly IRolApiClient _rolApi;

        public IndexModel(IModuloApiClient api, IBitacoraApiClient bitacora, IRolModuloApiClient rolModuloApi, IRolApiClient rolApi)
        {
            _api = api;
            _bitacora = bitacora;
            _rolModuloApi = rolModuloApi;
            _rolApi = rolApi;
        }

        public List<ModuloDto> Modulos { get; set; } = new();

        // SWEETALERT
        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? StatusType { get; set; }

        //   GET - Mostrar módulos
        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return Redirect("/HU_MICHEL/Login");

            try
            {
                //Obtener lista de módulos
                Modulos = await _api.GetAllAsync();

                // Registrar bitácora (mantengo tu orden)
                int idUsuario = HttpContext.Session.GetInt32("UsuarioID") ?? 0;
                await _bitacora.RegistrarAsync(idUsuario, "Consulta lista de módulos");

                //Obtener roles (con tupla)
                var (ok, status, roles, msg) = await _rolApi.GetAllAsync(idUsuario);

                if (!ok || roles == null)
                {
                    StatusMessage = "No se pudieron cargar los roles.";
                    StatusType = "error";
                    return Page();
                }

                //Determinar estado real del módulo según permisos
                foreach (var modulo in Modulos)
                {
                    bool visibleParaAlgunRol = false;

                    foreach (var rol in roles)
                    {
                        var permisos = await _rolModuloApi.GetByRolAsync(rol.ID_Rol);

                        if (permisos.Any(p => p.ID_Modulo == modulo.ID_Modulo && p.PuedeVer))
                        {
                            visibleParaAlgunRol = true;
                            break;
                        }
                    }

                    // Estado basado en permisos, no en Modulo.Activo del backend
                    modulo.Activo = visibleParaAlgunRol;
                }
            }
            catch
            {
                StatusMessage = "No se pudieron cargar los módulos.";
                StatusType = "error";
            }

            return Page();
        }
    }
}
