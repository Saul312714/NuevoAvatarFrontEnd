using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Grupos
{
    public class IndexModel : PageModel
    {
        private readonly IGrupoApiClient _grupoApi;

        public IndexModel(IGrupoApiClient grupoApi)
        {
            _grupoApi = grupoApi;
        }

        public List<GrupoViewDto> Lista { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("AccessToken");
                var userId = HttpContext.Session.GetInt32("UsuarioID");

                if (token is null || userId is null)
                    return Redirect("/HU_MICHEL/Login");

                // 1. Obtener grupos desde la API
                var grupos = await _grupoApi.GetAllAsync(token);
                Console.WriteLine($"DEBUG - Grupos: {grupos.Count}");

                var cursos = await _grupoApi.GetCursosAsync(token);
                Console.WriteLine($"DEBUG - Cursos: {cursos.Count}");

                var profesores = await _grupoApi.GetProfesoresAsync(token);
                Console.WriteLine($"DEBUG - Profesores: {profesores.Count}");

                var periodos = await _grupoApi.GetPeriodosAsync(userId.Value, token);
                Console.WriteLine($"DEBUG - Periodos: {periodos.Count}");


                // 3. Construir vista final
                Lista = grupos.Select(g => new GrupoViewDto
                {
                    ID_Grupo = g.ID_Grupo,
                    Numero = g.Numero,
                    ID_Curso = g.ID_Curso,
                    ID_Profesor = g.ID_Profesor,
                    ID_Periodo = g.ID_Periodo,
                    Horario = g.Horario,

                    NombreCurso = cursos.FirstOrDefault(c => c.ID_Curso == g.ID_Curso)?.Nombre ?? "—",
                    NombreProfesor = profesores.FirstOrDefault(p => p.ID_Profesor == g.ID_Profesor)?.Nombre ?? "—",
                    NombrePeriodo = periodos.FirstOrDefault(pe => pe.ID_Periodo == g.ID_Periodo)?.Nombre ?? "—"
                }).ToList();

                return Page();
            }
            catch (Exception)
            {
                ErrorMessage = "Error al cargar los grupos.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("AccessToken");
                var userId = HttpContext.Session.GetInt32("UsuarioID");

                if (token is null || userId is null)
                    return Redirect("/HU_MICHEL/Login");

                var response = await _grupoApi.DeleteAsync(id, userId.Value, token);

                //  Si el backend rechaza la eliminación (ADM13: matriculas asociadas)
                if (!response.ok)
                {
                    ErrorMessage = response.error ??
                                   "No se pudo eliminar el grupo, puede tener matrículas asociadas.";

                    // Vuelve a cargar grupos para mantener la tabla visible
                    await OnGetAsync();
                    return Page();
                }

                // ✔ Eliminación exitosa → PRG Pattern
                return RedirectToPage();
            }
            catch
            {
                ErrorMessage = "Ocurrió un error al intentar eliminar el grupo.";
                await OnGetAsync();
                return Page();
            }
        }
    }
}
