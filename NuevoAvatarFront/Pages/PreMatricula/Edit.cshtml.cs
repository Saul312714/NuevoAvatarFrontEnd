using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.PreMatriculas
{
    public class EditModel : PageModel
    {
        private readonly IPreMatriculaService _prematricula;
        private readonly ILogger<EditModel> _logger;

        // 👇 mismos servicios que usas en CreateModel
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
            IPeriodoService periodo)
        {
            _prematricula = prematricula;
            _logger = logger;
            _estudiante = estudiante;
            _carrera = carrera;
            _curso = curso;
            _periodo = periodo;
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
            try
            {
                var existente = await _prematricula.GetByIdAsync(id);
                if (existente is null)
                {
                    MensajeError = "No se encontró la prematrícula solicitada.";
                    return RedirectToPage("./Index");
                }

                PreMatricula = existente;

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

                await _prematricula.UpdateAsync(id, PreMatricula);
                MensajeExito = "Prematrícula actualizada correctamente.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar prematrícula");

                var mensajeError = ex.Message;
                if (ex.Message.Contains("periodos FUTUROS", StringComparison.OrdinalIgnoreCase))
                {
                    mensajeError = "Error: Solo se pueden prematricular periodos futuros (fecha de inicio posterior a la actual)";
                }

                ModelState.AddModelError(string.Empty, mensajeError);
                MensajeError = mensajeError;

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
                
                var estudiantes = await _estudiante.GetAllAsync();

                Estudiantes = estudiantes
                    .OrderBy(e => e.Nombre)
                    .Select(e => new SelectListItem
                    {
                        Value = e.ID_Estudiante.ToString(),
                        Text = $"{e.ID_Estudiante} - {e.Nombre}"
                    })
                    .ToList();

                if (!Estudiantes.Any())
                {
                    Estudiantes = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "1", Text = "Estudiante Demo 1" },
                        new SelectListItem { Value = "2", Text = "Estudiante Demo 2" }
                    };
                }

                var carrerasApi = await _carrera.GetAllAsync();

                Carreras = carrerasApi
                    .OrderBy(c => c.Nombre)
                    .Select(c => new SelectListItem
                    {
                        Value = c.ID_Carrera.ToString(),
                        Text = $"{c.ID_Carrera} - {c.Nombre}"
                    })
                    .ToList();

                if (!Carreras.Any())
                {
                    Carreras = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "1", Text = "Carrera Demo 1" },
                        new SelectListItem { Value = "2", Text = "Carrera Demo 2" }
                    };
                }

                
                var cursosApi = await _curso.GetNivel1Async(); 

                Cursos = cursosApi
                    .OrderBy(c => c.Nombre)
                    .Select(c => new SelectListItem
                    {
                        Value = c.ID_Curso.ToString(),
                        Text = $"{c.ID_Curso} - {c.Nombre} (Nivel {c.Nivel})"
                    })
                    .ToList();

                if (!Cursos.Any())
                {
                    Cursos = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "1", Text = "Curso Demo 1" },
                        new SelectListItem { Value = "2", Text = "Curso Demo 2" }
                    };
                }

               
                var periodos = await _periodo.GetPeriodosFuturosAsync();

                PeriodosFuturos = periodos
                    .OrderBy(p => p.Anno).ThenBy(p => p.Numero)
                    .Select(p => new SelectListItem
                    {
                        Value = p.ID_Periodo.ToString(),
                        Text = $"{p.Anno} - Período {p.Numero} (Inicio: {p.Fecha_Inicio:dd/MM/yyyy})"
                    })
                    .ToList();

                if (!PeriodosFuturos.Any())
                {
                    PeriodosFuturos = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "1", Text = "2025 - Periodo 1" },
                        new SelectListItem { Value = "2", Text = "2025 - Periodo 2" }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos para dropdowns en edición");
            }
        }
    }
}
