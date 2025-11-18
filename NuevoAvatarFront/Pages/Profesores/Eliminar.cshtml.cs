using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Profesores;

public class EliminarModel : PageModel
{
    private readonly TokenHelper _tokenHelper;
    private readonly ProfesorClient _api;
    private readonly ILogger<EliminarModel> _logger;

    // Inyectamos el TokenHelper para validar el token antes de proceder
    public EliminarModel(TokenHelper tokenHelper, ProfesorClient api, ILogger<EliminarModel> logger)
    {
        _tokenHelper = tokenHelper;
        _api = api;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public int id { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        // Validación de token
        var token = await _tokenHelper.EnsureValidAccessTokenAsync();
        if (token == null)
        {
            // Si no hay token válido, redirigimos al login
            return RedirectToPage("/HU_MICHEL/Login/Index");
        }

        try
        {
            // Intentamos eliminar el profesor
            var ok = await _api.DeleteAsync(id);

            if (ok)
                TempData["ok"] = $"Profesor {id} eliminado.";
            else
                TempData["err"] = $"No se pudo eliminar el profesor {id}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar el profesor con ID: {id}");
            TempData["err"] = $"Error al eliminar el profesor: {ex.Message}";
        }

        // Después de intentar la eliminación, redirigimos al índice
        return RedirectToPage("./Index");
    }
}

