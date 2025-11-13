using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.PreMatriculas
{
    public class IndexModel : PageModel
    {
        private readonly IPreMatriculaService _prematricula;
        private readonly ILogger<IndexModel> _logger;

        // Para servicios adicionales que necesites para los filtros
        // private readonly IPeriodoService _periodo;
        // private readonly ICarreraService _carrera;
        // private readonly ICursoService _curso;
        // private readonly IEstudianteService _estudiante;

        public IndexModel(
            IPreMatriculaService prematricula,
            ILogger<IndexModel> logger)
        {
            _prematricula = prematricula;
            _logger = logger;
        }

        public List<PreMatriculaDTO> PreMatriculas { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? FiltroPeriodo { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FiltroCarrera { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FiltroCurso { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FiltroEstudiante { get; set; }

        [TempData]
        public string? MensajeExito { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        // Listas para los dropdowns de filtros
        // public List<SelectListItem> Periodos { get; set; } = new();
        // public List<SelectListItem> Carreras { get; set; } = new();
        // public List<SelectListItem> Cursos { get; set; } = new();
        // public List<SelectListItem> Estudiantes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                
                var todasPrematriculas = await _prematricula.GetAllAsync();

                
                PreMatriculas = todasPrematriculas.Where(p =>
                    (!FiltroPeriodo.HasValue || p.ID_Periodo == FiltroPeriodo.Value) &&
                    (!FiltroCarrera.HasValue || p.ID_Carrera == FiltroCarrera.Value) &&
                    (!FiltroCurso.HasValue || p.ID_Curso == FiltroCurso.Value) &&
                    (!FiltroEstudiante.HasValue || p.ID_Estudiante == FiltroEstudiante.Value)
                ).ToList();

                // Aquí cargarías los datos para los dropdowns
                // await CargarDatosDropdowns();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar las prematrículas");
                MensajeError = "Error al cargar las prematrículas: " + ex.Message;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var resultado = await _prematricula.DeleteAsync(id);

                if (resultado)
                {
                    MensajeExito = "Prematrícula eliminada correctamente";
                }
                else
                {
                    MensajeError = "No se encontró la prematrícula a eliminar";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar prematrícula {id}");
                MensajeError = "Error al eliminar la prematrícula: " + ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetLimpiarFiltrosAsync()
        {
            FiltroPeriodo = null;
            FiltroCarrera = null;
            FiltroCurso = null;
            FiltroEstudiante = null;

            return RedirectToPage();
        }

        // Método auxiliar para cargar datos de los dropdowns
        /*
        private async Task CargarDatosDropdowns()
        {
            try
            {
                // Ejemplo de cómo cargar periodos futuros
                // var periodos = await _periodo.GetPeriodosFuturosAsync();
                // Periodos = periodos.Select(p => new SelectListItem
                // {
                //     Value = p.ID_Periodo.ToString(),
                //     Text = $"{p.Anno} - Periodo {p.Numero}"
                // }).ToList();

                // Similar para carreras, cursos, estudiantes...
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos para dropdowns");
            }
        }
        */
    }
}