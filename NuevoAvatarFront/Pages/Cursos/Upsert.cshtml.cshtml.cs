using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        // DTO principal del formulario
        [BindProperty]
        public CursoCreateUpdateDto Curso { get; set; } = new();

        // Para cargar lista de carreras
        public List<CarreraDto> Carreras { get; set; } = new();

        // Para saber si es edición
        [BindProperty(SupportsGet = true)]
        public int? id { get; set; }

        public bool EsEdicion => id.HasValue;

        public async Task<IActionResult> OnGetAsync()
        {
            // cargar carreras primero
            Carreras = await _carreraApi.GetAllAsync();

            // Si es edición, cargar el curso
            if (EsEdicion)
            {
                var data = await _cursoApi.GetByIdAsync(id.Value);

                if (data == null)
                {
                    TempData["Error"] = "Curso no encontrado.";
                    return RedirectToPage("/Cursos/Index");
                }

                Curso = new CursoCreateUpdateDto
                {
                    Nombre = data.Nombre,
                    Nivel = data.Nivel,
                    ID_Carrera = data.ID_Carrera
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(Curso.Nombre))
            {
                ModelState.AddModelError("Curso.Nombre", "El nombre es obligatorio.");
            }

            if (Curso.Nivel < 1 || Curso.Nivel > 12)
            {
                ModelState.AddModelError("Curso.Nivel", "El nivel debe ser entre 1 y 12.");
            }

            if (!ModelState.IsValid)
            {
                Carreras = await _carreraApi.GetAllAsync();
                return Page();
            }

            bool resultado;

            if (EsEdicion)
            {
                resultado = await _cursoApi.UpdateAsync(id.Value, Curso);
            }
            else
            {
                resultado = await _cursoApi.CreateAsync(Curso);
            }

            if (!resultado)
            {
                TempData["Error"] = "No se pudo guardar el curso. Verifique los datos.";
                Carreras = await _carreraApi.GetAllAsync();
                return Page();
            }

            TempData["Success"] = "Curso guardado correctamente.";
            return RedirectToPage("/Cursos/Index");
        }
    }
}
