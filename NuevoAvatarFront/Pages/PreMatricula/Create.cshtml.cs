using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;        
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.PreMatriculas
{
    public class CreateModel : PageModel
    {
        private readonly IPreMatriculaService _prematricula;
        private readonly ILogger<CreateModel> _logger;
        private readonly IEstudianteService _estudiante;
        private readonly ICarreraService _carrera;
        private readonly ICursoService _curso;
        private readonly IPeriodoService _periodo;
        private readonly TokenHelper _tokenHelper;

        public CreateModel(
            IPreMatriculaService prematricula,
            ILogger<CreateModel> logger,
            IEstudianteService estudiante,
            ICarreraService carrera,
            ICursoService curso,
            IPeriodoService periodo,
            TokenHelper tokenHelper)
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

        public List<SelectListItem> Estudiantes { get; set; } = new();
        public List<SelectListItem> Carreras { get; set; } = new();
        public List<SelectListItem> Cursos { get; set; } = new();
        public List<SelectListItem> PeriodosFuturos { get; set; } = new();

        
        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index"); 

            try
            {
                await CargarDatosDropdowns();
                return Page();
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al cargar datos para crear prematrícula");
                MensajeError = apiEx.Message;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos para crear prematrícula");
                MensajeError = "Error al cargar los datos necesarios: " + ex.Message;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index"); 

            if (!ModelState.IsValid)
            {
                await CargarDatosDropdowns();
                return Page();
            }

            try
            {
                await _prematricula.CreateAsync(PreMatricula);
                TempData["MensajeExito"] = "Prematrícula creada exitosamente";
                return RedirectToPage("./Index");
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al crear prematrícula");

                var mensajeError = apiEx.Message;
                MensajeError = mensajeError;
                ModelState.AddModelError(string.Empty, mensajeError);

                await CargarDatosDropdowns();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear prematrícula");

                var mensajeError = ex.Message;
                if (ex.Message.Contains("periodos FUTUROS", StringComparison.OrdinalIgnoreCase))
                {
                    mensajeError = "Error: Solo se pueden prematricular periodos futuros (fecha de inicio posterior a la actual)";
                }

                MensajeError = mensajeError;
                ModelState.AddModelError(string.Empty, mensajeError);

                await CargarDatosDropdowns();
                return Page();
            }
        }

        private async Task CargarDatosDropdowns()
        {
            try
            {
                var estudiantesApi = await _estudiante.GetAllAsync();

                Estudiantes = estudiantesApi
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

                var cursosNivel1 = cursosApi
                    .Where(c => c.Nivel == 1);

                Cursos = cursosNivel1
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
                    .OrderBy(p => p.Anno)
                    .ThenBy(p => p.Numero)
                    .Select(p => new SelectListItem
                    {
                        Value = p.ID_Periodo.ToString(),
                        Text = p.NombreMostrar
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
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al cargar datos para dropdowns (Create Prematrícula)");
                MensajeError = apiEx.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos para dropdowns");
            }
        }
    }
}
