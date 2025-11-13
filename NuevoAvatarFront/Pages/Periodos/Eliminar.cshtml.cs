using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Periodos
{
    public class EliminarModel : PageModel
    {
        private readonly PeriodoClient _api;
        public EliminarModel(PeriodoClient api) { _api = api; }

        [BindProperty(SupportsGet = true)]
        public int id { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var ok = await _api.DeleteAsync(id);
            TempData[ok ? "ok" : "err"] = ok
                ? "Período eliminado correctamente."
                : "No se pudo eliminar el período.";
            return RedirectToPage("./Index");
        }
    }
}