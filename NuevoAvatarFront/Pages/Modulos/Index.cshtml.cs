using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Modulos
{
    public class IndexModel : PageModel
    {
        private readonly IModuloApiClient _api;

        public IndexModel(IModuloApiClient api)
        {
            _api = api;
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
                Modulos = await _api.GetAllAsync();
            }
            catch
            {
                StatusMessage = "No se pudieron cargar los módulos.";
                StatusType = "error";
            }

            return Page();
        }

        //   POST - Activar módulo
        public async Task<IActionResult> OnPostActivarAsync(int idModulo)
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return Redirect("/HU_MICHEL/Login");

            var modulo = await _api.GetByIdAsync(idModulo);

            if (modulo == null)
            {
                StatusMessage = "El módulo no existe.";
                StatusType = "error";
                return RedirectToPage();
            }

            try
            {
                modulo.Activo = true;

                var updated = await _api.UpdateAsync(modulo);

                if (updated == null)
                {
                    StatusMessage = "No se pudo activar el módulo.";
                    StatusType = "error";
                }
                else
                {
                    StatusMessage = "Módulo activado correctamente.";
                    StatusType = "success";
                }
            }
            catch
            {
                StatusMessage = "Error al activar el módulo.";
                StatusType = "error";
            }

            return RedirectToPage();
        }
    }
}
