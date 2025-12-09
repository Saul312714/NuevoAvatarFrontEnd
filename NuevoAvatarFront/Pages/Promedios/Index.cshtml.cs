using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Services;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Models;

namespace NuevoAvatarFront.Pages.Promedios
{
    public class IndexModel : PageModel
    {
        private readonly IEstudianteService _estudianteService;
        private readonly IGrupoService _grupoService;
        private readonly ICursoService _cursoService;
        private readonly INotasApiClient _notasService;

        public IndexModel(
            IEstudianteService estudianteService,
            IGrupoService grupoService,
            ICursoService cursoService,
            INotasApiClient notasService)
        {
            _estudianteService = estudianteService;
            _grupoService = grupoService;
            _cursoService = cursoService;
            _notasService = notasService;
        }

        // Parámetros que pueden venir en la URL (desde Notas)
        [BindProperty(SupportsGet = true)]
        public int IdEstudiante { get; set; }

        [BindProperty(SupportsGet = true)]
        public int IdGrupo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Identificacion { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string TipoIdentificacion { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string NombreCurso { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public decimal PromedioFinal { get; set; }

        // Filtros ADM18
        [BindProperty]
        public string Periodo { get; set; } = string.Empty;

        public List<string> Periodos { get; set; } = new();

        public bool BusquedaRealizada { get; set; }

        public List<PromedioDto> Resultados { get; set; } = new();

        public async Task OnGet()
        {
            Periodos = new List<string> { "2025-1", "2025-2" };

            // 1) Si viene desde Notas con IdEstudiante, rellenar identificación y tipo
            if (IdEstudiante > 0)
            {
                // Si ya vinieron por query (asp-route) se usan tal cual
                if (!string.IsNullOrEmpty(Identificacion) && !string.IsNullOrEmpty(TipoIdentificacion))
                {
                    // Solo rellenar el formulario, no buscar aún
                }
                else
                {
                    // Si solo vino el IdEstudiante, buscamos al estudiante para rellenar
                    var estudiantes = await _estudianteService.GetAllAsync();
                    var est = estudiantes.FirstOrDefault(e => e.ID_Estudiante == IdEstudiante);

                    if (est != null)
                    {
                        if (string.IsNullOrEmpty(Identificacion))
                            Identificacion = est.Identificacion;

                        if (string.IsNullOrEmpty(TipoIdentificacion))
                            TipoIdentificacion = est.TipoIdentificacion;
                    }
                }
            }

            // 2) Si además vino promedio y nombre de curso, mostrar directamente el resultado
            if (PromedioFinal > 0 && !string.IsNullOrEmpty(NombreCurso))
            {
                BusquedaRealizada = true;

                Resultados = new List<PromedioDto>
                {
                    new PromedioDto
                    {
                        CodigoCurso = "", // opcional: se puede rellenar si lo mandas desde Notas
                        NombreCurso = NombreCurso,
                        Promedio = Math.Round(PromedioFinal, 2)
                    }
                };
            }
        }

        public async Task<IActionResult> OnPostBuscar()
        {
            BusquedaRealizada = true;

            if (string.IsNullOrWhiteSpace(Identificacion))
            {
                ModelState.AddModelError("", "Debe ingresar una identificación.");
                return Page();
            }

            // 1. Obtener estudiante por identificación
            var estudiantes = await _estudianteService.GetAllAsync();
            var est = estudiantes.FirstOrDefault(e =>
                e.Identificacion.Equals(Identificacion, StringComparison.OrdinalIgnoreCase));

            if (est == null)
            {
                ModelState.AddModelError("", "No se encontró el estudiante.");
                return Page();
            }

            int idEst = est.ID_Estudiante;

            // 2. Obtener todos los grupos del sistema
            var grupos = await _grupoService.GetAllAsync();
            var cursos = await _cursoService.GetAllAsync();

            // 3. Recorrer grupos y calcular promedio real por curso
            foreach (var g in grupos)
            {
                var notas = await _notasService.ObtenerNotasAsync(idEst, g.ID_Grupo, 1);
                var rubros = await _notasService.ObtenerDesgloseAsync(g.ID_Grupo, 1);

                if (!rubros.Any())
                    continue;

                decimal promedio = 0;

                foreach (var r in rubros)
                {
                    var nota = notas.FirstOrDefault(n => n.ID_Rubro == r.ID_Rubro);

                    if (nota != null)
                        promedio += nota.Valor * (r.Porcentaje / 100m);
                }

                Resultados.Add(new PromedioDto
                {
                    CodigoCurso = cursos.First(c => c.ID_Curso == g.ID_Curso).ID_Curso.ToString(),
                    NombreCurso = cursos.First(c => c.ID_Curso == g.ID_Curso).Nombre,
                    Promedio = Math.Round(promedio, 2)
                });
            }

            return Page();
        }

        // EXPORTACION CSV 
        public IActionResult OnPostExportCsv()
        {
            var csv = "Curso,Promedio\n";

            foreach (var item in Resultados)
            {
                csv += $"{item.NombreCurso},{item.Promedio}\n";
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "Promedios.csv");
        }

        // EXPORTACION PDF 
        public IActionResult OnPostExportPdf()
        {
            return Content("PDF no implementado aún.");
        }
    }

    // DTO OFICIAL ADM18 -----------------------------------------------------
    public class PromedioDto
    {
        public string CodigoCurso { get; set; }
        public string NombreCurso { get; set; }
        public decimal Promedio { get; set; }
    }
}
