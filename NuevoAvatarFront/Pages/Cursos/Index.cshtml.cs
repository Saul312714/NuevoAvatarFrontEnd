using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Cursos
{
    public class IndexModel : PageModel
    {
        private readonly ICursoApiClient _cursoApi;

        public IndexModel(ICursoApiClient cursoApi)
        {
            _cursoApi = cursoApi;
        }

        public List<CursoDto> Cursos { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Ya no se usa token aquí, ApiClient lo obtiene solo
            Cursos = await _cursoApi.GetAllAsync();

            return Page();
        }

        public async Task<IActionResult> OnGetDeleteAsync(int id)
        {
            bool eliminado = await _cursoApi.DeleteAsync(id);

            if (!eliminado)
            {
                TempData["Error"] = "No se pudo eliminar. Puede estar asociado a grupos.";
                return RedirectToPage();
            }

            TempData["Success"] = "Curso eliminado correctamente.";
            return RedirectToPage();
        }
    }
}
