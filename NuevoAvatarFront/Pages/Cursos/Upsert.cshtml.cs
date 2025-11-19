using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Cursos
{
    public class UpsertModel : PageModel
    {
        private readonly ICursoApiClient _cursoApi;
        private readonly ICarreraApiClient _carreraApi;

        public UpsertModel(ICursoApiClient cursoApi, ICarreraApiClient carreraApi)
        {
            _cursoApi = cursoApi;
            _carreraApi = carreraApi;
        }

        [BindProperty]
        public CursoDto Curso { get; set; } = new();

        public List<SelectListItem> Carreras { get; set; } = new();
        public bool EsEdicion => Curso.ID_Curso > 0;

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        // GET
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // cargar carreras
            await CargarCarrerasAsync();

            if (id is null)
                return Page();

            // cargar curso
            var curso = await _cursoApi.GetByIdAsync(id.Value);
            if (curso is null)
                return RedirectToPage("/Cursos/Index");

            Curso = curso;

            return Page();
        }

        // POST
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            await CargarCarrerasAsync();

            // VALIDACIONES PERSONALIZADAS
            if (string.IsNullOrWhiteSpace(Curso.Nombre))
                ModelState.AddModelError("Curso.Nombre", "El nombre es requerido.");

            if (Curso.Nivel < 1 || Curso.Nivel > 12)
                ModelState.AddModelError("Curso.Nivel", "El nivel debe estar entre 1 y 12.");

            if (Curso.ID_Carrera <= 0)
                ModelState.AddModelError("Curso.ID_Carrera", "Debe seleccionar una carrera.");

            if (!ModelState.IsValid)
                return Page();

            if (id.HasValue && id.Value > 0)
            {
                var dto = new CursoUpdateDto
                {
                    Nombre = Curso.Nombre.Trim(),
                    Nivel = Curso.Nivel,
                    ID_Carrera = Curso.ID_Carrera
                };

                var result = await _cursoApi.UpdateAsync(Curso.ID_Curso, dto);

                if (!result.Ok)
                {
                    ModelState.AddModelError(string.Empty, result.Msg);
                    return Page();
                }
            }
            else
            {
                var dto = new CursoCreateDto
                {
                    Nombre = Curso.Nombre.Trim(),
                    Nivel = Curso.Nivel,
                    ID_Carrera = Curso.ID_Carrera
                };

                var result = await _cursoApi.CreateAsync(dto);

                if (!result.Ok)
                {
                    ModelState.AddModelError(string.Empty, result.Msg);
                    return Page();
                }
            }

            return RedirectToPage("/Cursos/Index");
        }

        private async Task CargarCarrerasAsync()
        {
            var carreras = await _carreraApi.GetAllAsync();

            Carreras = carreras
                .Select(c => new SelectListItem(c.Nombre, c.ID_Carrera.ToString()))
                .ToList();
        }
    }
}
