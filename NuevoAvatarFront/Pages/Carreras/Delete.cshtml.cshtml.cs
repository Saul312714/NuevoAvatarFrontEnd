using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Carreras
{
    public class DeleteModel : PageModel
    {
        private readonly ICarreraApiClient _carreraService;
        private readonly IBitacoraApiClient _bitacoraService;

        public DeleteModel(ICarreraApiClient carreraService, IBitacoraApiClient bitacoraService)
        {
            _carreraService = carreraService;
            _bitacoraService = bitacoraService;
        }

        public CarreraDto? Carrera { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Carrera = await _carreraService.GetByIdAsync(id);
            return Carrera == null ? RedirectToPage("Index") : Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var carrera = await _carreraService.GetByIdAsync(id);
            if (carrera == null)
                return RedirectToPage("Index");

            var ok = await _carreraService.DeleteAsync(id);
            if (ok)
            {
                await _bitacoraService.RegistrarAsync(GetUsuarioId(), $"Eliminó carrera: {carrera.Nombre}");
                return RedirectToPage("Index", new { institucionId = carrera.InstitucionId });
            }

            TempData["error"] = "No se puede eliminar: existen cursos vinculados.";
            return RedirectToPage("Index", new { institucionId = carrera.InstitucionId });
        }

        private int GetUsuarioId() =>
            HttpContext.Session.GetInt32("UsuarioID") ?? 0;
    }
}
