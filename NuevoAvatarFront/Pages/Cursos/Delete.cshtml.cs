using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Cursos
{
    public class DeleteModel : PageModel
    {
        private readonly ICursoApiClient _cursoApi;
        private readonly ICarreraApiClient _carreraApi;

        public DeleteModel(ICursoApiClient cursoApi, ICarreraApiClient carreraApi)
        {
            _cursoApi = cursoApi;
            _carreraApi = carreraApi;
        }

        [BindProperty]
        public CursoDto? Curso { get; set; }

        public string NombreCarrera { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;


        // GET ? Mostrar información del curso
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Curso = await _cursoApi.GetByIdAsync(id);

            if (Curso is null)
                return RedirectToPage("/Cursos/Index");

            // Obtener nombre de carrera
            var carrera = await _carreraApi.GetByIdAsync(Curso.ID_Carrera);
            NombreCarrera = carrera?.Nombre ?? "(Carrera desconocida)";

            return Page();
        }


        // POST Confirmar eliminación
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostAsync(int id)
        {
            // Intentar eliminar
            var result = await _cursoApi.DeleteAsync(id);

            if (!result.Ok)
            {
                ErrorMessage = result.Msg;

                // Recargar información para volver a mostrar la vista
                Curso = await _cursoApi.GetByIdAsync(id);
                var carrera = await _carreraApi.GetByIdAsync(Curso.ID_Carrera);
                NombreCarrera = carrera?.Nombre ?? string.Empty;

                return Page();
            }

            return RedirectToPage("/Cursos/Index");
        }
    }
}
