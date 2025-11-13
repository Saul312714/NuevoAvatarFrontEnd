using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Cursos
{
    public class EditModel : PageModel
    {
        private readonly ICursoApiClient _cursoService;
        private readonly ICarreraApiClient _carreraService;
        private readonly IBitacoraApiClient _bitacoraService;

        public EditModel(ICursoApiClient cursoService, ICarreraApiClient carreraService, IBitacoraApiClient bitacoraService)
        {
            _cursoService = cursoService;
            _carreraService = carreraService;
            _bitacoraService = bitacoraService;
        }

        [BindProperty]
        public CursoCreateUpdateDto Form { get; set; } = new();

        public int Id { get; set; }
        public List<CarreraDto> Carreras { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var curso = await _cursoService.GetByIdAsync(id);
            if (curso == null)
                return RedirectToPage("Index");

            Id = id;
            Form.Nombre = curso.Nombre;
            Form.Nivel = curso.Nivel;
            Form.CarreraId = curso.CarreraId;

            Carreras = await _carreraService.GetAllAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                Carreras = await _carreraService.GetAllAsync();
                return Page();
            }

            var ok = await _cursoService.UpdateAsync(id, Form);
            if (ok)
            {
                await _bitacoraService.RegistrarAsync(GetUsuarioId(), $"Actualizó el curso {Form.Nombre}");
                return RedirectToPage("Index");
            }

            TempData["error"] = "Error al actualizar el curso";
            return Page();
        }

        private int GetUsuarioId() =>
            HttpContext.Session.GetInt32("UsuarioID") ?? 0;
    }
}
