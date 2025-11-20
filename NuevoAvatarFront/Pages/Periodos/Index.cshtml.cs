using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Periodos
{
    public class IndexModel : PageModel
    {
        private readonly PeriodoClient _api;
        private readonly TokenHelper _tokenHelper;
        private readonly ILogger<IndexModel> _logger; 

        public IndexModel(PeriodoClient api, TokenHelper tokenHelper, ILogger<IndexModel> logger)
        {
            _api = api;
            _tokenHelper = tokenHelper;
            _logger = logger;                          
        }

        public List<PeriodoDto> Periodos { get; private set; } = new();
        public int Total { get; private set; }

        [BindProperty(SupportsGet = true)] public string? q { get; set; }
        [BindProperty(SupportsGet = true)] public string? sort { get; set; } = "inicio";
        [BindProperty(SupportsGet = true)] public int page { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int size { get; set; } = 10;

        public IEnumerable<SelectListItem> SortOptions { get; private set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> SizeOptions { get; private set; } = Enumerable.Empty<SelectListItem>();

        public async Task<IActionResult> OnGet()         
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            page = page <= 0 ? 1 : page;
            size = size <= 0 ? 10 : size;

            SortOptions = new[]
            {
                new SelectListItem("Inicio",  "inicio",  sort == "inicio"),
                new SelectListItem("Fin",     "fin",     sort == "fin"),
                new SelectListItem("Año",     "anio",    sort == "anio"),
                new SelectListItem("Número",  "numero",  sort == "numero"),
            };
            SizeOptions = new[]
            {
                new SelectListItem("10", "10", size == 10),
                new SelectListItem("25", "25", size == 25),
                new SelectListItem("50", "50", size == 50),
            };

            try
            {
                IEnumerable<PeriodoDto> datos = await _api.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var t = q.Trim().ToLower();
                    datos = datos.Where(p =>
                        p.Anno.ToString().Contains(t) ||
                        p.Numero.ToString().Contains(t) ||
                        p.Fecha_Inicio.ToString("yyyy-MM-dd").Contains(t) ||
                        p.Fecha_Fin.ToString("yyyy-MM-dd").Contains(t));
                }

                datos = sort switch
                {
                    "fin" => datos.OrderBy(p => p.Fecha_Fin),
                    "anio" => datos.OrderBy(p => p.Anno),
                    "numero" => datos.OrderBy(p => p.Numero),
                    _ => datos.OrderBy(p => p.Fecha_Inicio),
                };

                Total = datos.Count();
                Periodos = datos
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToList();

                return Page();
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al obtener períodos");
                TempData["error"] = apiEx.Message;

                Periodos = new();
                Total = 0;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener períodos");
                TempData["error"] = "Error al cargar los períodos. Intente nuevamente más tarde.";

                Periodos = new();
                Total = 0;
                return Page();
            }
        }

        public string EstadoDe(PeriodoDto p)
        {
            var hoy = DateTime.Today;
            if (p.Fecha_Inicio.Date > hoy) return "Futuro";
            if (hoy >= p.Fecha_Inicio.Date && hoy <= p.Fecha_Fin.Date) return "Activo";
            return "Cerrado";
        }

        public string EstadoClase(string estado) => estado switch
        {
            "Futuro" => "badge rounded-pill bg-warning text-dark",
            "Activo" => "badge rounded-pill bg-success",
            _ => "badge rounded-pill bg-danger"
        };
    }
}
