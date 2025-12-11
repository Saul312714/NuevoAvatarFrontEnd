using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Pagos
{
    public class IndexModel : PageModel
    {
        private readonly IPagoApiClient _pagoApi;
        private readonly IBitacoraApiClient _bitacoraApi;

        public IndexModel(IPagoApiClient pagoApi, IBitacoraApiClient bitacoraApi)
        {
            _pagoApi = pagoApi;
            _bitacoraApi = bitacoraApi;
        }

        public List<PagoDto> Pagos { get; set; } = new();

        // Filtros
        [BindProperty(SupportsGet = true)]
        public int? Anio { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Mes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Estado { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("AccessToken");
                var userId = HttpContext.Session.GetInt32("UsuarioID");

                if (token is null || userId is null)
                    return Redirect("/HU_MICHEL/Login");

                if (!Anio.HasValue)
                    Anio = DateTime.Now.Year;

                if (!Mes.HasValue)
                    Mes = DateTime.Now.Month;

                var lista = await _pagoApi.GetAllPagosAsync(token);

                var query = lista.AsQueryable();

                if (Anio.HasValue)
                    query = query.Where(p => p.FechaPago.Year == Anio.Value);

                if (Mes.HasValue)
                    query = query.Where(p => p.FechaPago.Month == Mes.Value);

                if (!string.IsNullOrWhiteSpace(Estado) && Estado != "Todos")
                    query = query.Where(p =>
                        string.Equals(p.Estado, Estado, StringComparison.OrdinalIgnoreCase));

                Pagos = query
                    .OrderByDescending(p => p.FechaPago)
                    .ToList();

                // Bitácora: consulta de pagos
                await _bitacoraApi.RegistrarAsync(userId.Value, "Consulta de listado de pagos");

                return Page();
            }
            catch
            {
                ErrorMessage = "Ocurrió un error al cargar los pagos.";
                return Page();
            }
        }
    }
}
