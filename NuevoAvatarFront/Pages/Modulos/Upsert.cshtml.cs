using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Modulos
{
    public class UpsertModel : PageModel
    {
        private readonly IModuloApiClient _api;

        public UpsertModel(IModuloApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public ModuloDto Modulo { get; set; } = new();

        public bool IsEdit => Modulo.ID_Modulo > 0;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return Redirect("/HU_MICHEL/Login");

            if (id == null)
                return Page();

            var data = await _api.GetByIdAsync(id.Value);

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
            if (!ModelState.IsValid)
                return Page();

            try
            {
                if (IsEdit)
                    await _api.UpdateAsync(Modulo);
                else
                    await _api.CreateAsync(Modulo);

                TempData["success"] = "Módulo guardado correctamente.";
            }
            catch
            {
                TempData["error"] = "Error al guardar el módulo.";
                return Page();
            }

            return RedirectToPage("/Modulos/Index");
        }
    }
}
