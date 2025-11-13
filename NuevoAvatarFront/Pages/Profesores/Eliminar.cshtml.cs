using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Profesores;

public class EliminarModel : PageModel
{
    private readonly ProfesorClient _api;
    public EliminarModel(ProfesorClient api) { _api = api; }

    [BindProperty(SupportsGet = true)]
    public int id { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var ok = await _api.DeleteAsync(id);

        if (ok) TempData["ok"] = $"Profesor {id} eliminado.";
        else TempData["err"] = $"No se pudo eliminar el profesor {id}.";

        return RedirectToPage("./Index");
    }
}
