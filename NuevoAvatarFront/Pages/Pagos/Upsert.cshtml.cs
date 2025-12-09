using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Pagos
{
    public class UpsertModel : PageModel
    {
        private readonly IPagoApiClient _pagoApi;
        private readonly IBitacoraApiClient _bitacoraApi;

        public UpsertModel(IPagoApiClient pagoApi, IBitacoraApiClient bitacoraApi)
        {
            _pagoApi = pagoApi;
            _bitacoraApi = bitacoraApi;
        }

        [BindProperty]
        public int IdFactura { get; set; }

        [BindProperty]
        public decimal Monto { get; set; }

        public List<FacturaDto> Facturas { get; set; } = new();

        public decimal? SaldoPendiente { get; set; }
        public decimal? TotalFactura { get; set; }
        public string? EstadoFactura { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? idFactura)
        {
            try
            {
                var token = HttpContext.Session.GetString("AccessToken");
                var userId = HttpContext.Session.GetInt32("UsuarioID");

                if (token is null || userId is null)
                    return Redirect("/HU_MICHEL/Login");

                Facturas = await _pagoApi.GetFacturasAsync(token);

                if (idFactura.HasValue)
                {
                    IdFactura = idFactura.Value;
                    await CargarResumenFactura(token, IdFactura);
                }

                // Bitácora: acceso a pantalla de registro
                await _bitacoraApi.RegistrarAsync(userId.Value, "Ingreso a pantalla de registro de pagos");

                return Page();
            }
            catch
            {
                ErrorMessage = "Ocurrió un error al cargar la información de facturas.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("AccessToken");
                var userId = HttpContext.Session.GetInt32("UsuarioID");

                if (token is null || userId is null)
                    return Redirect("/HU_MICHEL/Login");

                // Recargar catálogos para la vista
                Facturas = await _pagoApi.GetFacturasAsync(token);

                if (IdFactura <= 0)
                {
                    ErrorMessage = "Debe seleccionar una factura.";
                    return Page();
                }

                if (Monto <= 0)
                {
                    ErrorMessage = "El monto del pago debe ser mayor a cero.";
                    await CargarResumenFactura(token, IdFactura);
                    return Page();
                }

                var factura = await _pagoApi.GetFacturaByIdAsync(IdFactura, token);

                if (factura is null)
                {
                    ErrorMessage = "La factura seleccionada no existe.";
                    return Page();
                }

                EstadoFactura = factura.Estado;
                TotalFactura = factura.Total;

                // Validación de estado (ejemplo: no permitir sobre 'Anulada')
                if (factura.Estado.Equals("Anulada", StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = "No se pueden registrar pagos sobre una factura anulada.";
                    await CargarResumenFactura(token, IdFactura);
                    return Page();
                }

                // Calcular saldo pendiente basado en pagos aplicados
                var todosPagos = await _pagoApi.GetAllPagosAsync(token);
                var pagosAplicados = todosPagos
                    .Where(p => p.ID_Factura == IdFactura &&
                                p.Estado.Equals("Aplicado", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var totalPagado = pagosAplicados.Sum(p => p.Monto);
                var saldo = factura.Total - totalPagado;

                SaldoPendiente = saldo;

                if (saldo <= 0)
                {
                    ErrorMessage = "La factura ya no tiene saldo pendiente.";
                    return Page();
                }

                if (Monto > saldo)
                {
                    ErrorMessage = $"El monto no puede ser mayor al saldo pendiente ({saldo:N2}).";
                    return Page();
                }

                var (ok, error) = await _pagoApi.RegistrarPagoAsync(
                    IdFactura,
                    Monto,
                    userId.Value,
                    token);

                if (!ok)
                {
                    ErrorMessage = string.IsNullOrWhiteSpace(error)
                        ? "No se pudo registrar el pago."
                        : error;
                    return Page();
                }

                // Bitácora: registro de pago
                await _bitacoraApi.RegistrarAsync(
                    userId.Value,
                    $"Registró un pago de {Monto:N2} para la factura {IdFactura}");

                return RedirectToPage("./Index");
            }
            catch
            {
                ErrorMessage = "Ocurrió un error inesperado al registrar el pago.";
                return Page();
            }
        }

        private async Task CargarResumenFactura(string token, int idFactura)
        {
            var factura = await _pagoApi.GetFacturaByIdAsync(idFactura, token);
            if (factura is null)
                return;

            EstadoFactura = factura.Estado;
            TotalFactura = factura.Total;

            var todosPagos = await _pagoApi.GetAllPagosAsync(token);
            var pagosAplicados = todosPagos
                .Where(p => p.ID_Factura == idFactura &&
                            p.Estado.Equals("Aplicado", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var totalPagado = pagosAplicados.Sum(p => p.Monto);
            SaldoPendiente = factura.Total - totalPagado;
        }
    }
}
