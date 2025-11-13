using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Cursos
{
    public class DeleteModel : PageModel
    {
        private readonly ICursoApiClient _cursoService;
        private readonly IBitacoraApiClient _bitacoraService;

        public DeleteModel(ICursoApiClient cursoService, IBitacoraApiClient bitacoraService)
        {
            _cursoService = cursoService;
            _bitacoraService = bitacoraService;
        }

        public CursoDto? Curso { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Curso = await _cursoService.GetByIdAsync(id);
            return Curso == null ? RedirectToPage("Index") : Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var curso = await _cursoService.GetByIdAsync(id);
            if (curso == null)
                return RedirectToPage("Index");

            // Verificamos si hay grupos asociados
            if (curso.ID_Curso > 0)
            {
                TempData["error"] = "No se puede eliminar: el curso tiene grupos asociados.";
                return RedirectToPage("Index");
            }

            var ok = await _cursoService.DeleteAsync(id);
            if (ok)
            {
                await _bitacoraService.RegistrarAsync(GetUsuarioId(), $"Eliminó el curso {curso.Nombre}");
                return RedirectToPage("Index");
            }

            TempData["error"] = "Error al eliminar el curso";
            return RedirectToPage("Index");
        }

        private int GetUsuarioId() =>
            HttpContext.Session.GetInt32("UsuarioID") ?? 0;
    }
}
