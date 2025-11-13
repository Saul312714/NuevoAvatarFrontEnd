using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Carreras
{
    public class DeleteModel : PageModel
    {
        private readonly ICarreraApiClient _carreraApi;

        public DeleteModel(ICarreraApiClient carreraApi)
        {
            _carreraApi = carreraApi;
        }

        // IMPORTANTE: Debe soportar GET
        [BindProperty(SupportsGet = true)]
        public int ID_Carrera { get; set; }

        public CarreraDto? Carrera { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // El valor llega automáticamente desde ?id=
            Carrera = await _carreraApi.GetByIdAsync(ID_Carrera);

            if (Carrera == null)
            {
                TempData["Error"] = "La carrera no existe.";
                return RedirectToPage("/Carreras/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var ok = await _carreraApi.DeleteAsync(ID_Carrera);

            if (ok)
                TempData["Success"] = "Carrera eliminada correctamente.";
            else
                TempData["Error"] = "No se pudo eliminar la carrera.";

            return RedirectToPage("/Carreras/Index");
        }
    }
}

