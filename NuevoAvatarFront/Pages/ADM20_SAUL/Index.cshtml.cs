using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.ADM20_SAUL
{
    public class IndexModel : PageModel
    {
        private readonly IFacturaFrontService _facturaService;
        private readonly IPeriodoService _periodoService;
        private readonly IEstudianteService _estudianteService;
        private readonly IMatriculaFrontService _matriculaService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IFacturaFrontService facturaService,
            IPeriodoService periodoService,
            IEstudianteService estudianteService,
            IMatriculaFrontService matriculaService,
            ILogger<IndexModel> logger)
        {
            _facturaService = facturaService;
            _periodoService = periodoService;
            _estudianteService = estudianteService;
            _matriculaService = matriculaService;
            _logger = logger;

            Estados = new List<SelectListItem>
            {
                new("Todos", ""),
                new("Pendiente", "Pendiente"),
                new("Pagada", "Pagada"),
                new("Anulada", "Anulada")
            };
        }

        // ======================
        // FILTROS
        // ======================
        [BindProperty(SupportsGet = true)]
        public int? FiltroPeriodo { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FiltroEstudiante { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroEstado { get; set; }

        public List<SelectListItem> Estados { get; set; }
        public List<SelectListItem> Periodos { get; set; } = new();
        public List<SelectListItem> Estudiantes { get; set; } = new();
        public List<SelectListItem> Matriculas { get; set; } = new();

        // ======================
        // LISTADO
        // ======================
        public List<FacturaDto> Facturas { get; set; } = new();

        // ======================
        // CREACIÓN
        // ======================
        [BindProperty]
        public int NuevoIdPeriodo { get; set; }

        [BindProperty]
        public int NuevoIdEstudiante { get; set; }

        [BindProperty]
        public int NuevoIdMatricula { get; set; }

        // ======================
        // REVERSAR
        // ======================
        [BindProperty]
        public int FacturaIdReversar { get; set; }

        [BindProperty]
        public string MotivoReversion { get; set; } = string.Empty;

        [TempData] public string? MensajeError { get; set; }
        [TempData] public string? MensajeOk { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            await CargarCombosAsync();
            MarcarEstadoSeleccionado();
            await CargarListadoAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            await CargarCombosAsync();
            MarcarEstadoSeleccionado();

            if (NuevoIdPeriodo <= 0 || NuevoIdEstudiante <= 0 || NuevoIdMatricula <= 0)
            {
                MensajeError = "Debe indicar período, estudiante y matrícula válidos.";
                await CargarListadoAsync();
                return Page();
            }

            try
            {
                var idFactura = await _facturaService.CrearAsync(
                    NuevoIdEstudiante,
                    NuevoIdMatricula,
                    NuevoIdPeriodo);

                MensajeOk = $"Factura {idFactura} creada correctamente.";

                return RedirectToPage(new
                {
                    FiltroPeriodo,
                    FiltroEstudiante,
                    FiltroEstado
                });
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al crear factura ADM20");
                MensajeError = apiEx.Message;
                await CargarListadoAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear factura ADM20");
                MensajeError = "Ocurrió un error al crear la factura. Intente nuevamente.";
                await CargarListadoAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostReversarAsync()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            await CargarCombosAsync();
            MarcarEstadoSeleccionado();

            if (FacturaIdReversar <= 0 || string.IsNullOrWhiteSpace(MotivoReversion))
            {
                MensajeError = "Debe seleccionar una factura válida y escribir el motivo de reversión.";
                await CargarListadoAsync();
                return Page();
            }

            try
            {
                await _facturaService.ReversarAsync(FacturaIdReversar, MotivoReversion);

                MensajeOk = $"Factura {FacturaIdReversar} anulada correctamente.";

                return RedirectToPage(new
                {
                    FiltroPeriodo,
                    FiltroEstudiante,
                    FiltroEstado
                });
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al reversar factura ADM20");
                MensajeError = apiEx.Message;
                await CargarListadoAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al reversar factura ADM20");
                MensajeError = "Ocurrió un error al reversar la factura. Intente nuevamente.";
                await CargarListadoAsync();
                return Page();
            }
        }

        private async Task CargarCombosAsync()
        {
            try
            {
                var periodos = await _periodoService.GetAllAsync();
                Periodos = periodos
                    .Select(p => new SelectListItem
                    {
                        Value = p.ID_Periodo.ToString(),
                        Text = $"{p.ID_Periodo}"
                    })
                    .ToList();

                var estudiantes = await _estudianteService.GetAllAsync();
                Estudiantes = estudiantes
                    .Select(e => new SelectListItem
                    {
                        Value = e.ID_Estudiante.ToString(),
                        Text = $"{e.ID_Estudiante} - {e.Nombre}"
                    })
                    .ToList();

                var matriculas = await _matriculaService.ListarAsync();
                Matriculas = matriculas
                    .Select(m => new SelectListItem
                    {
                        Value = m.ID_Matricula.ToString(),
                        Text = $"{m.ID_Matricula}"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar combos de ADM20");
                Periodos ??= new List<SelectListItem>();
                Estudiantes ??= new List<SelectListItem>();
                Matriculas ??= new List<SelectListItem>();
            }
        }

        private async Task CargarListadoAsync()
        {
            try
            {
                string? estado = string.IsNullOrWhiteSpace(FiltroEstado) || FiltroEstado == "Todos"
                    ? null
                    : FiltroEstado;

                var lista = await _facturaService.ListarAsync(FiltroPeriodo, FiltroEstudiante, estado);

                if (FiltroPeriodo.HasValue)
                {
                    lista = lista
                        .Where(f => f.ID_Periodo == FiltroPeriodo.Value)
                        .ToList();
                }

                Facturas = lista;
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al listar facturas ADM20");
                MensajeError = apiEx.Message;
                Facturas = new List<FacturaDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al listar facturas ADM20");
                MensajeError = "Ocurrió un error al obtener las facturas.";
                Facturas = new List<FacturaDto>();
            }
        }

        private void MarcarEstadoSeleccionado()
        {
            var valor = FiltroEstado ?? "";
            foreach (var it in Estados)
            {
                it.Selected = string.Equals(it.Value, valor, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
