using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Cursos
{
    public class CreateModel : PageModel
    {
        private readonly ICursoApiClient _cursoService;
        private readonly ICarreraApiClient _carreraService;
        private readonly IBitacoraApiClient _bitacoraService;

        public CreateModel(ICursoApiClient cursoService, ICarreraApiClient carreraService, IBitacoraApiClient bitacoraService)
        {
            _cursoService = cursoService;
            _carreraService = carreraService;
            _bitacoraService = bitacoraService;
        }

        [BindProperty]
        public CursoCreateUpdateDto Form { get; set; } = new();

        public List<CarreraDto> Carreras { get; set; } = new();

        public async Task OnGetAsync()
        {
            Carreras = await _carreraService.GetAllAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Carreras = await _carreraService.GetAllAsync();
                return Page();
            }

            var ok = await _cursoService.CreateAsync(Form);
            if (ok)
            {
                await _bitacoraService.RegistrarAsync(GetUsuarioId(), $"Creó el curso {Form.Nombre}");
                TempData["ok"] = "Curso creado correctamente";
                return RedirectToPage("Index");
            }

            TempData["error"] = "Error al crear el curso";
            return Page();
        }

        private int GetUsuarioId() =>
            HttpContext.Session.GetInt32("UsuarioID") ?? 0;
    }
}
