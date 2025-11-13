using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Profesores;

public class UpsertModel : PageModel
{
    private readonly ProfesorClient _api;
    public UpsertModel(ProfesorClient api) { _api = api; }

    [BindProperty]
    public ProfesorDto Profesor { get; set; } = new();

    public async Task OnGet(int? id)
    {
        if (id.HasValue && id.Value > 0)
        {
            var data = await _api.GetByIdAsync(id.Value);
            if (data is not null) Profesor = data;
        }
    }

    public async Task<IActionResult> OnPostSave()
    {
        if (!ModelState.IsValid) return Page();

        bool ok = Profesor.ID_Profesor == 0
            ? await _api.CreateAsync(Profesor)
            : await _api.UpdateAsync(Profesor);

        if (ok) TempData["ok"] = "Profesor guardado correctamente.";
        else TempData["err"] = "No se pudo guardar el profesor.";

        return RedirectToPage("./Index");
    }
}