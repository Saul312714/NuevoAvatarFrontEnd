using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PdfDocument = QuestPDF.Fluent.Document;

namespace NuevoAvatarFront.Pages.ADM19_SAUL
{
    public class IndexModel : PageModel
    {
        private readonly IMatriculaFrontService _matriculaService;
        private readonly IPeriodoService _periodoService;
        private readonly ICarreraService _carreraService;
        private readonly ICursoService _cursoService;
        private readonly IGrupoService _grupoService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IMatriculaFrontService matriculaService,
            IPeriodoService periodoService,
            ICarreraService carreraService,
            ICursoService cursoService,
            IGrupoService grupoService,
            ILogger<IndexModel> logger)
        {
            _matriculaService = matriculaService;
            _periodoService = periodoService;
            _carreraService = carreraService;
            _cursoService = cursoService;
            _grupoService = grupoService;
            _logger = logger;
        }

        public List<SelectListItem> Periodos { get; set; } = new();
        public List<SelectListItem> Carreras { get; set; } = new();
        public List<SelectListItem> Cursos { get; set; } = new();
        public List<SelectListItem> Grupos { get; set; } = new();

        public List<CarreraDto> CarrerasData { get; set; } = new();
        public List<CursoDTO> CursosData { get; set; } = new();

        public List<SelectListItem> SizeOptions { get; } = new()
        {
            new("10", "10"),
            new("25", "25"),
            new("50", "50")
        };

        public List<SelectListItem> SortOptions { get; } = new()
        {
            new("ID Matrícula", "id"),
            new("Nombre",      "nombre"),
            new("Identificación", "identificacion"),
            new("Carrera",     "carrera"),
            new("Curso",       "curso"),
            new("Grupo",       "grupo")
        };

        [BindProperty(SupportsGet = true)] public int? FiltroPeriodo { get; set; }
        [BindProperty(SupportsGet = true)] public int? FiltroCarrera { get; set; }
        [BindProperty(SupportsGet = true)] public int? FiltroCurso { get; set; }
        [BindProperty(SupportsGet = true)] public int? FiltroGrupo { get; set; }

        [BindProperty(SupportsGet = true)] public int page { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int size { get; set; } = 10;
        [BindProperty(SupportsGet = true)] public string? sort { get; set; }
        [BindProperty(SupportsGet = true)] public bool asc { get; set; } = true;

        public int Total { get; set; }
        public List<Adm19ListadoRowDto> Resultados { get; set; } = new();

        [TempData] public string? MensajeError { get; set; }
        [TempData] public string? MensajeOk { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            await CargarCombosAsync();

            var vieneDeFormulario =
                Request.Query.ContainsKey(nameof(FiltroPeriodo)) ||
                Request.Query.ContainsKey(nameof(FiltroCarrera)) ||
                Request.Query.ContainsKey(nameof(FiltroCurso)) ||
                Request.Query.ContainsKey(nameof(FiltroGrupo));

            if (!vieneDeFormulario || !FiltroPeriodo.HasValue)
            {
                MensajeError = null;
                MensajeOk = null;
                Resultados = new List<Adm19ListadoRowDto>();
                Total = 0;
                return Page();
            }

            try
            {
                var (datos, total) = await _matriculaService.GetListadoAdm19Async(
                    FiltroPeriodo,
                    idCarrera: null,
                    FiltroCurso,
                    FiltroGrupo,
                    page,
                    size,
                    sort,
                    asc);

                if (FiltroCarrera.HasValue)
                {
                    var idsCursosCarrera = CursosData
                        .Where(c => c.ID_Carrera == FiltroCarrera.Value)
                        .Select(c => c.ID_Curso)
                        .ToHashSet();

                    datos = datos
                        .Where(d => idsCursosCarrera.Contains(d.ID_Curso))
                        .ToList();

                    total = datos.Count;
                }

                Resultados = datos;
                Total = total;
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al obtener listado ADM19");
                MensajeError = apiEx.Message;

                if (apiEx.Errors != null)
                {
                    foreach (var e in apiEx.Errors.Where(x => !string.IsNullOrWhiteSpace(x)))
                        ModelState.AddModelError(string.Empty, e);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener listado ADM19");
                MensajeError = "Ocurrió un error al obtener el listado. Intente nuevamente más tarde.";
            }

            return Page();
        }

        public async Task<IActionResult> OnGetExportCsvAsync()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            if (!FiltroPeriodo.HasValue)
            {
                MensajeError = "Debe seleccionar al menos un período para exportar.";
                return RedirectToPage(new { FiltroPeriodo, FiltroCarrera, FiltroCurso, FiltroGrupo, page, size, sort, asc });
            }

            try
            {
                await CargarCombosAsync();

                var (datos, _) = await _matriculaService.GetListadoAdm19Async(
                    FiltroPeriodo,
                    idCarrera: null,
                    FiltroCurso,
                    FiltroGrupo,
                    page,
                    size,
                    sort,
                    asc);

                if (FiltroCarrera.HasValue)
                {
                    var idsCursosCarrera = CursosData
                        .Where(c => c.ID_Carrera == FiltroCarrera.Value)
                        .Select(c => c.ID_Curso)
                        .ToHashSet();

                    datos = datos
                        .Where(d => idsCursosCarrera.Contains(d.ID_Curso))
                        .ToList();
                }

                var sb = new StringBuilder();
                sb.AppendLine("ID_Matricula;ID_Estudiante;Tipo_Identificacion;Identificacion;Nombre;ID_Carrera;Carrera;ID_Curso;Curso;ID_Grupo;Numero_Grupo;ID_Periodo");

                foreach (var r in datos)
                {
                    var curso = CursosData.FirstOrDefault(c => c.ID_Curso == r.ID_Curso);
                    var idCarrera = curso?.ID_Carrera ?? 0;
                    var nombreCarrera = ObtenerNombreCarreraPorCurso(r.ID_Curso);
                    var nombreCurso = ObtenerNombreCurso(r.ID_Curso);
                    var textoGrupo = ObtenerTextoGrupo(r.ID_Grupo);

                    sb.AppendLine(
                        $"{r.ID_Matricula};{r.ID_Estudiante};{r.Tipo_Identificacion};{r.Identificacion};{r.Nombre};" +
                        $"{idCarrera};{nombreCarrera};{r.ID_Curso};{nombreCurso};{r.ID_Grupo};{textoGrupo};{r.ID_Periodo}");
                }

                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                var fileName = $"Listado_Matriculas_Periodo_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al exportar CSV ADM19");
                MensajeError = "Ocurrió un error al exportar el CSV. Intente nuevamente más tarde.";
                return RedirectToPage(new { FiltroPeriodo, FiltroCarrera, FiltroCurso, FiltroGrupo, page, size, sort, asc });
            }
        }

        public async Task<IActionResult> OnGetExportPdfAsync()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            if (!FiltroPeriodo.HasValue)
            {
                MensajeError = "Debe seleccionar al menos un período para exportar.";
                return RedirectToPage(new { FiltroPeriodo, FiltroCarrera, FiltroCurso, FiltroGrupo, page, size, sort, asc });
            }

            try
            {
                await CargarCombosAsync();

                var (datos, _) = await _matriculaService.GetListadoAdm19Async(
                    FiltroPeriodo,
                    idCarrera: null,
                    FiltroCurso,
                    FiltroGrupo,
                    page,
                    size,
                    sort,
                    asc);

                if (FiltroCarrera.HasValue)
                {
                    var idsCursosCarrera = CursosData
                        .Where(c => c.ID_Carrera == FiltroCarrera.Value)
                        .Select(c => c.ID_Curso)
                        .ToHashSet();

                    datos = datos
                        .Where(d => idsCursosCarrera.Contains(d.ID_Curso))
                        .ToList();
                }

                var fileName = $"Listado_Matriculas_Periodo_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var pdfBytes = GenerarPdfListado(datos);

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al exportar PDF ADM19");
                MensajeError = "Ocurrió un error al exportar el PDF. Intente nuevamente más tarde.";
                return RedirectToPage(new { FiltroPeriodo, FiltroCarrera, FiltroCurso, FiltroGrupo, page, size, sort, asc });
            }
        }

        private byte[] GenerarPdfListado(List<Adm19ListadoRowDto> datos)
        {
            using var stream = new MemoryStream();

            var document = PdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Listado de Matrículas por período")
                        .SemiBold().FontSize(16).FontColor(Colors.Blue.Medium);

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);  
                            columns.RelativeColumn(70);  
                            columns.RelativeColumn(80);  
                            columns.RelativeColumn(120); 
                            columns.RelativeColumn(110); 
                            columns.RelativeColumn(90);  
                            columns.RelativeColumn(50);  
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellHeader).Text("ID");
                            header.Cell().Element(CellHeader).Text("Tipo ID");
                            header.Cell().Element(CellHeader).Text("Identificación");
                            header.Cell().Element(CellHeader).Text("Nombre");
                            header.Cell().Element(CellHeader).Text("Carrera");
                            header.Cell().Element(CellHeader).Text("Curso");
                            header.Cell().Element(CellHeader).Text("Grupo");
                        });

                        foreach (var r in datos)
                        {
                            var carrera = ObtenerNombreCarreraPorCurso(r.ID_Curso);
                            var curso = ObtenerNombreCurso(r.ID_Curso);
                            var grupo = ObtenerTextoGrupo(r.ID_Grupo);

                            table.Cell().Element(CellBody).Text(r.ID_Matricula.ToString());
                            table.Cell().Element(CellBody).Text(r.Tipo_Identificacion);
                            table.Cell().Element(CellBody).Text(r.Identificacion);
                            table.Cell().Element(CellBody).Text(r.Nombre);
                            table.Cell().Element(CellBody).Text(carrera);
                            table.Cell().Element(CellBody).Text(curso);
                            table.Cell().Element(CellBody).Text(grupo);
                        }

                        static IContainer CellHeader(IContainer container)
                            => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Darken2);

                        static IContainer CellBody(IContainer container)
                            => container.PaddingVertical(2).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
                    });

                    page.Footer()
                        .AlignRight()
                        .Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                        });
                });
            });

            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        private async Task CargarCombosAsync()
        {
            try
            {
                if (HttpContext.Session.GetString("AccessToken") == null)
                    return;

                var periodos = await _periodoService.GetAllAsync();
                Periodos = periodos
                    .OrderBy(p => p.Anno).ThenBy(p => p.Numero)
                    .Select(p => new SelectListItem
                    {
                        Value = p.ID_Periodo.ToString(),
                        Text = $"{p.Anno} - Período {p.Numero} (Inicio: {p.Fecha_Inicio:dd/MM/yyyy})",
                        Selected = FiltroPeriodo.HasValue && FiltroPeriodo.Value == p.ID_Periodo
                    })
                    .ToList();

                var carreras = await _carreraService.GetAllAsync();
                CarrerasData = carreras;
                Carreras = carreras
                    .OrderBy(c => c.Nombre)
                    .Select(c => new SelectListItem
                    {
                        Value = c.ID_Carrera.ToString(),
                        Text = $"{c.ID_Carrera} - {c.Nombre}",
                        Selected = FiltroCarrera.HasValue && FiltroCarrera.Value == c.ID_Carrera
                    }).ToList();

                var cursos = await _cursoService.GetAllAsync();
                CursosData = cursos;
                Cursos = cursos
                    .OrderBy(c => c.Nombre)
                    .Select(c => new SelectListItem
                    {
                        Value = c.ID_Curso.ToString(),
                        Text = $"{c.ID_Curso} - {c.Nombre}",
                        Selected = FiltroCurso.HasValue && FiltroCurso.Value == c.ID_Curso
                    }).ToList();

                var grupos = await _grupoService.GetAllAsync();
                Grupos = grupos
                    .OrderBy(g => g.ID_Curso).ThenBy(g => g.Numero_Grupo)
                    .Select(g => new SelectListItem
                    {
                        Value = g.ID_Grupo.ToString(),
                        Text = $"Curso {g.ID_Curso} - Grupo {g.Numero_Grupo} ({g.Horario})",
                        Selected = FiltroGrupo.HasValue && FiltroGrupo.Value == g.ID_Grupo
                    }).ToList();

                foreach (var it in SizeOptions)
                    it.Selected = it.Value == size.ToString();

                foreach (var it in SortOptions)
                    it.Selected = string.Equals(it.Value, sort ?? "", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar combos de ADM19");
            }
        }

        [BindProperty(SupportsGet = true)]
        public int? FiltroPeriodoId
        {
            get => FiltroPeriodo;
            set => FiltroPeriodo = value;
        }

        [BindProperty(SupportsGet = true)]
        public int? FiltroCarreraId
        {
            get => FiltroCarrera;
            set => FiltroCarrera = value;
        }

        [BindProperty(SupportsGet = true)]
        public int? FiltroCursoId
        {
            get => FiltroCurso;
            set => FiltroCurso = value;
        }

        [BindProperty(SupportsGet = true)]
        public int? FiltroGrupoId
        {
            get => FiltroGrupo;
            set => FiltroGrupo = value;
        }

        public int TotalPages
        {
            get
            {
                var porPagina = Math.Max(size, 1);
                var pages = (int)Math.Ceiling((double)Total / porPagina);
                return pages < 1 ? 1 : pages;
            }
        }

        public string ObtenerNombreCarreraPorCurso(int idCurso)
        {
            var curso = CursosData.FirstOrDefault(c => c.ID_Curso == idCurso);
            if (curso is null)
                return "";

            var carrera = CarrerasData.FirstOrDefault(car => car.ID_Carrera == curso.ID_Carrera);
            return carrera?.Nombre ?? curso.ID_Carrera.ToString();
        }

        public string ObtenerNombreCarrera(int idCarrera)
        {
            var item = Carreras.FirstOrDefault(c => c.Value == idCarrera.ToString());
            return item?.Text ?? idCarrera.ToString();
        }

        public string ObtenerNombreCurso(int idCurso)
        {
            var item = Cursos.FirstOrDefault(c => c.Value == idCurso.ToString());
            return item?.Text ?? idCurso.ToString();
        }

        public string ObtenerTextoGrupo(int idGrupo)
        {
            var item = Grupos.FirstOrDefault(g => g.Value == idGrupo.ToString());
            if (item is null)
                return $"Grupo {idGrupo}";

            var texto = item.Text;
            var idx = texto.IndexOf("Grupo ", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var resto = texto.Substring(idx + "Grupo ".Length);
                var espacio = resto.IndexOf(' ');
                if (espacio > 0)
                    resto = resto.Substring(0, espacio);

                return "Grupo " + resto;
            }

            return texto;
        }
    }
}
