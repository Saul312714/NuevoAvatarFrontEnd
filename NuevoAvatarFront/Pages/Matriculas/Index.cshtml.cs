using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Matriculas
{
    public class IndexModel : PageModel
    {
        private readonly IMatriculaFrontService _matriculaService;
        private readonly IEstudianteService _estudianteService;
        private readonly ICursoService _cursoService;
        private readonly IPeriodoService _periodoService;
        private readonly ILogger<IndexModel> _logger;
        private readonly IGrupoService _grupoService;

        public IndexModel(
            IMatriculaFrontService matriculaService,
            IEstudianteService estudianteService,
            ICursoService cursoService,
            IPeriodoService periodoService,
            ILogger<IndexModel> logger,
            IGrupoService grupoService
        )
        {
            _matriculaService = matriculaService;
            _estudianteService = estudianteService;
            _cursoService = cursoService;
            _periodoService = periodoService;
            _logger = logger;
            _grupoService = grupoService;
        }

        public List<SelectListItem> Estudiantes { get; set; } = new();
        public List<SelectListItem> Cursos { get; set; } = new();
        public List<SelectListItem> PeriodosActivos { get; set; } = new();
        public List<SelectListItem> Grupos { get; set; } = new();

        [BindProperty]
        public MatriculaCreateDto NuevaMatricula { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? FiltroCurso { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FiltroGrupo { get; set; }

        public List<MatriculadoDto> Matriculados { get; set; } = new();

        [TempData]
        public string? MensajeExito { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // 🔥 VALIDAR SESIÓN
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            try
            {
                await CargarCombosAsync();

                if (FiltroCurso.HasValue && FiltroGrupo.HasValue)
                {
                    Matriculados = await _matriculaService
                        .GetMatriculadosPorCursoGrupoAsync(FiltroCurso.Value, FiltroGrupo.Value);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la pantalla de matrículas");
                MensajeError = "Error al cargar la información de matrículas.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostMatricularAsync()
        {
            // 🔥 VALIDAR SESIÓN
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync();
                return Page();
            }

            try
            {
                var idGenerado = await _matriculaService.CrearAsync(NuevaMatricula);

                MensajeExito = $"Matrícula registrada correctamente (ID: {idGenerado}).";

                return RedirectToPage(new
                {
                    FiltroCurso = NuevaMatricula.ID_Curso,
                    FiltroGrupo = NuevaMatricula.ID_Grupo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear matrícula");

                MensajeError = ex.Message;
                ModelState.AddModelError(string.Empty, ex.Message);

                await CargarCombosAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnGetExportCsvAsync()
        {
            // 🔥 VALIDAR SESIÓN
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            if (!FiltroCurso.HasValue || !FiltroGrupo.HasValue)
            {
                MensajeError = "Debe seleccionar un curso y un grupo para exportar el CSV.";
                return RedirectToPage(new { FiltroCurso, FiltroGrupo });
            }

            try
            {
                var data = await _matriculaService
                    .GetMatriculadosPorCursoGrupoAsync(FiltroCurso.Value, FiltroGrupo.Value);

                if (!data.Any())
                {
                    MensajeError = "No hay datos para exportar con esos filtros.";
                    return RedirectToPage(new { FiltroCurso, FiltroGrupo });
                }

                var sb = new StringBuilder();

                sb.AppendLine("ID_Estudiante;Identificacion;Nombre;Email;ID_Curso;ID_Grupo;ID_Periodo");

                foreach (var m in data)
                {
                    sb.AppendLine(
                        $"{m.ID_Estudiante};{m.Identificacion};{m.Nombre};{m.Email};{m.ID_Curso};{m.ID_Grupo};{m.ID_Periodo}");
                }

                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                var fileName = $"Matriculados_Curso{FiltroCurso}_Grupo{FiltroGrupo}.csv";

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar CSV de matrículas");
                MensajeError = "Error al exportar el CSV de matrículas.";
                return RedirectToPage(new { FiltroCurso, FiltroGrupo });
            }
        }

        private async Task CargarCombosAsync()
        {
            try
            {
                // 🔥 SI NO HAY TOKEN, NO INTENTAR CARGAR NADA
                if (HttpContext.Session.GetString("AccessToken") == null)
                    return;

                var estudiantes = await _estudianteService.GetAllAsync();
                Estudiantes = estudiantes
                    .OrderBy(e => e.Nombre)
                    .Select(e => new SelectListItem
                    {
                        Value = e.ID_Estudiante.ToString(),
                        Text = $"{e.ID_Estudiante} - {e.Nombre}"
                    })
                    .ToList();

                var cursos = await _cursoService.GetAllAsync();
                Cursos = cursos
                    .OrderBy(c => c.Nombre)
                    .Select(c => new SelectListItem
                    {
                        Value = c.ID_Curso.ToString(),
                        Text = $"{c.ID_Curso} - {c.Nombre} (Nivel {c.Nivel})"
                    })
                    .ToList();

                var periodos = await _periodoService.GetPeriodosFuturosAsync();
                PeriodosActivos = periodos
                    .Where(p => p.Fecha_Inicio > DateTime.Now)
                    .OrderBy(p => p.Anno).ThenBy(p => p.Numero)
                    .Select(p => new SelectListItem
                    {
                        Value = p.ID_Periodo.ToString(),
                        Text = $"{p.Anno} - Período {p.Numero} (Inicio: {p.Fecha_Inicio:dd/MM/yyyy})"
                    })
                    .ToList();

                var grupos = await _grupoService.GetAllAsync();
                Grupos = grupos
                    .OrderBy(g => g.ID_Curso)
                    .ThenBy(g => g.Numero_Grupo)
                    .Select(g => new SelectListItem
                    {
                        Value = g.ID_Grupo.ToString(),
                        Text = $"Curso {g.ID_Curso} - Grupo {g.Numero_Grupo} ({g.Horario})"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos para los combos de matrículas");
            }
        }
    }
}
