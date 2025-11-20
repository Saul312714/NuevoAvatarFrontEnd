using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Modulos
{
    public class DeleteModel : PageModel
    {
        private readonly IModuloApiClient _api;

        public DeleteModel(IModuloApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public ModuloDto Modulo { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return Redirect("/HU_MICHEL/Login");

            var data = await _api.GetByIdAsync(id);

            if (data == null)
            {
                TempData["error"] = "El módulo no existe.";
                return RedirectToPage("/Modulos/Index");
            }

            Modulo = data!;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // ?? Convertir Eliminar ? Desactivar
                Modulo.Activo = false;

                await _api.UpdateAsync(Modulo);

                TempData["success"] = "Módulo desactivado correctamente.";
            }
            catch
            {
                TempData["error"] = "Error al desactivar el módulo.";
            }

            return RedirectToPage("/Modulos/Index");
        }
    }
}
