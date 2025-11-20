using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Infra;      
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Periodos
{
    public class EliminarModel : PageModel
    {
        private readonly PeriodoClient _api;
        private readonly ILogger<EliminarModel> _logger;   

        public EliminarModel(PeriodoClient api, ILogger<EliminarModel> logger)
        {
            _api = api;
            _logger = logger;                              
        }

        [BindProperty(SupportsGet = true)]
        public int id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            try
            {
                var ok = await _api.DeleteAsync(id);

                TempData[ok ? "ok" : "err"] = ok
                    ? "Período eliminado correctamente."
                    : "No se pudo eliminar el período.";

                return RedirectToPage("./Index");
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al eliminar período {Id}", id);
                TempData["err"] = apiEx.Message;
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar período {Id}", id);
                TempData["err"] = "Error al eliminar el período. Intente nuevamente más tarde.";
                return RedirectToPage("./Index");
            }
        }
    }
}

