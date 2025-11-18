using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.PreMatriculas
{
    public class EditModel : PageModel
    {
        private readonly IPreMatriculaService _prematricula;
        private readonly ILogger<EditModel> _logger;
        private readonly TokenHelper _tokenHelper;

        // Servicios adicionales que usas en CreateModel
        private readonly IEstudianteService _estudiante;
        private readonly ICarreraService _carrera;
        private readonly ICursoService _curso;
        private readonly IPeriodoService _periodo;

        public EditModel(
            IPreMatriculaService prematricula,
            ILogger<EditModel> logger,
            IEstudianteService estudiante,
            ICarreraService carrera,
            ICursoService curso,
            IPeriodoService periodo,
            TokenHelper tokenHelper
        )
        {
            _prematricula = prematricula;
            _logger = logger;
            _estudiante = estudiante;
            _carrera = carrera;
            _curso = curso;
            _periodo = periodo;
            _tokenHelper = tokenHelper;
        }

        [BindProperty]
        public PreMatriculaDTO PreMatricula { get; set; } = new();

        [TempData]
        public string? MensajeError { get; set; }

        [TempData]
        public string? MensajeExito { get; set; }

        public List<SelectListItem> Estudiantes { get; set; } = new();
        public List<SelectListItem> Carreras { get; set; } = new();
        public List<SelectListItem> Cursos { get; set; } = new();
        public List<SelectListItem> PeriodosFuturos { get; set; } = new();

        // GET: cargamos la prematrícula y los dropdowns
        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index"); // Redirige si no hay token

            try
            {
                // Obtener la prematrícula
                var existente = await _prematricula.GetByIdAsync(id);
                if (existente is null)
                {
                    MensajeError = "No se encontró la prematrícula solicitada.";
                    return RedirectToPage("./Index");
                }

                PreMatricula = existente;

                // Cargar los datos para los dropdowns
                await CargarDatosDropdowns();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la prematrícula para editar");
                MensajeError = "Error al cargar la prematrícula: " + ex.Message;
                return RedirectToPage("./Index");
            }
        }

        // POST: guardar cambios
        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosDropdowns();
                return Page();
            }

            try
            {
                PreMatricula.ID_Prematricula = id;

                // Llamar al servicio para actualizar la prematrícula
                await _prematricula.UpdateAsync(id, PreMatricula);
                MensajeExito = "Prematrícula actualizada correctamente.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar prematrícula");

                // Manejo de errores relacionados con los periodos futuros
                var mensajeError = ex.Message;
                if (ex.Message.Contains("periodos FUTUROS", StringComparison.OrdinalIgnoreCase))
                {
                    mensajeError = "Error: Solo se pueden prematricular periodos futuros (fecha de inicio posterior a la actual)";
                }

                ModelState.AddModelError(string.Empty, mensajeError);
                MensajeError = mensajeError;

                // Recargar los dropdowns
                await CargarDatosDropdowns();
                return Page();
            }
        }

        // ==========================
        //  Cargar combos reutilizado
        // ==========================
        private async Task CargarDatosDropdowns()
        {
            try
            {
                // Cargar estudiantes
                var estudiantes = await _estudiante.GetAllAsync();
                Estudiantes = estudiantes
                    .OrderBy(e => e.Nombre)
                    .Select(e => new SelectListItem
                    {
                        Value = e.ID_Estudiante.ToString(),
                        Text = $"{e.ID_Estudiante} - {e.Nombre}"
                    })
                    .ToList();

                // Cargar carreras
                var carreras = await _carrera.GetAllAsync();
                Carreras = carreras
                    .OrderBy(c => c.Nombre)
                    .Select(c => new SelectListItem
                    {
                        Value = c.ID_Carrera.ToString(),
                        Text = $"{c.ID_Carrera} - {c.Nombre}"
                    })
                    .ToList();

                // Cargar cursos (solo de nivel 1)
                var cursos = await _curso.GetNivel1Async();  // Suponiendo que hay un método para obtener cursos de nivel 1
                Cursos = cursos
                    .OrderBy(c => c.Nombre)
                    .Select(c => new SelectListItem
                    {
                        Value = c.ID_Curso.ToString(),
                        Text = $"{c.ID_Curso} - {c.Nombre} (Nivel {c.Nivel})"
                    })
                    .ToList();

                // Cargar periodos futuros
                var periodos = await _periodo.GetPeriodosFuturosAsync();
                PeriodosFuturos = periodos
                    .OrderBy(p => p.Anno)
                    .ThenBy(p => p.Numero)
                    .Select(p => new SelectListItem
                    {
                        Value = p.ID_Periodo.ToString(),
                        Text = $"{p.Anno} - Período {p.Numero} (Inicio: {p.Fecha_Inicio:dd/MM/yyyy})"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos para dropdowns en edición");
            }
        }
    }
}

