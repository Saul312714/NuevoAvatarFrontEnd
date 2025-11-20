using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Profesores
{
    public class EliminarModel : PageModel
    {
        private readonly TokenHelper _tokenHelper;
        private readonly ProfesorClient _api;
        private readonly ILogger<EliminarModel> _logger;

        public EliminarModel(
            TokenHelper tokenHelper,
            ProfesorClient api,
            ILogger<EliminarModel> logger)
        {
            _tokenHelper = tokenHelper;
            _api = api;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();
            if (token == null)
            {
                return RedirectToPage("/HU_MICHEL/Login/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();
            if (token == null)
            {
                return RedirectToPage("/HU_MICHEL/Login/Index");
            }

            try
            {
                var ok = await _api.DeleteAsync(id);

                if (ok)
                {
                    TempData["ok"] = $"Profesor {id} eliminado.";
                }
                else
                {
                    TempData["err"] = $"No se pudo eliminar el profesor {id}.";
                }
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al eliminar profesor {Id}", id);
                TempData["err"] = apiEx.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el profesor con ID: {Id}", id);
                TempData["err"] =
                    "Ocurrió un error al eliminar el profesor. Intente nuevamente más tarde.";
            }

            return RedirectToPage("./Index");
        }
    }
}
