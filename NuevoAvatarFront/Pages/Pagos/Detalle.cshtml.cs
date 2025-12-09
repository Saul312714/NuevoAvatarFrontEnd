using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Pagos
{
    public class DetalleModel : PageModel
    {
        private readonly IPagoApiClient _pagoApi;
        private readonly IBitacoraApiClient _bitacoraApi;

        public DetalleModel(IPagoApiClient pagoApi, IBitacoraApiClient bitacoraApi)
        {
            _pagoApi = pagoApi;
            _bitacoraApi = bitacoraApi;
        }

        public PagoDto? Pago { get; set; }
        public FacturaDto? Factura { get; set; }

        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("AccessToken");
                var userId = HttpContext.Session.GetInt32("UsuarioID");

                if (token is null || userId is null)
                    return Redirect("/HU_MICHEL/Login");

                Pago = await _pagoApi.GetPagoByIdAsync(id, token);

                if (Pago is null)
                {
                    TempData["ErrorMessage"] = "El pago solicitado no existe.";
                    return RedirectToPage("./Index");
                }

                Factura = await _pagoApi.GetFacturaByIdAsync(Pago.ID_Factura, token);

                if (Factura != null)
                {
                    var todosPagos = await _pagoApi.GetAllPagosAsync(token);
                    var pagosAplicados = todosPagos
                        .Where(p => p.ID_Factura == Factura.ID_Factura &&
                                    p.Estado.Equals("Aplicado", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    TotalPagado = pagosAplicados.Sum(p => p.Monto);
                    SaldoPendiente = Factura.Total - TotalPagado;
                }

                // Bitácora: consulta detalle
                await _bitacoraApi.RegistrarAsync(
                    userId.Value,
                    $"Consulta detalle de pago {id}");

                return Page();
            }
            catch
            {
                ErrorMessage = "Ocurrió un error al cargar el detalle del pago.";
                return Page();
            }
        }
    }
}
