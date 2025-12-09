using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Pagos
{
    public class ReversarModel : PageModel
    {
        private readonly IPagoApiClient _pagoApi;
        private readonly IBitacoraApiClient _bitacoraApi;

        public ReversarModel(IPagoApiClient pagoApi, IBitacoraApiClient bitacoraApi)
        {
            _pagoApi = pagoApi;
            _bitacoraApi = bitacoraApi;
        }

        [BindProperty]
        public int IdPago { get; set; }

        [BindProperty]
        public string Motivo { get; set; } = string.Empty;

        public PagoDto? Pago { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("AccessToken");
                var userId = HttpContext.Session.GetInt32("UsuarioID");

                if (token is null || userId is null)
                    return Redirect("/HU_MICHEL/Login");

                IdPago = id;
                Pago = await _pagoApi.GetPagoByIdAsync(id, token);

                if (Pago is null)
                {
                    TempData["ErrorMessage"] = "El pago solicitado no existe.";
                    return RedirectToPage("./Index");
                }

                if (!Pago.Estado.Equals("Aplicado", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "Solo se pueden reversar pagos aplicados.";
                    return RedirectToPage("./Index");
                }

                return Page();
            }
            catch
            {
                TempData["ErrorMessage"] = "Ocurrió un error al cargar la información del pago.";
                return RedirectToPage("./Index");
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

                if (string.IsNullOrWhiteSpace(Motivo))
                {
                    ErrorMessage = "Debe indicar un motivo para la reversa.";
                    Pago = await _pagoApi.GetPagoByIdAsync(IdPago, token);
                    return Page();
                }

                var pago = await _pagoApi.GetPagoByIdAsync(IdPago, token);

                if (pago is null)
                {
                    TempData["ErrorMessage"] = "El pago ya no existe.";
                    return RedirectToPage("./Index");
                }

                if (!pago.Estado.Equals("Aplicado", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "Solo se pueden reversar pagos aplicados.";
                    return RedirectToPage("./Index");
                }

                var (ok, error) = await _pagoApi.ReversarPagoAsync(IdPago, userId.Value, token);

                if (!ok)
                {
                    ErrorMessage = string.IsNullOrWhiteSpace(error)
                        ? "No se pudo reversar el pago."
                        : error;

                    Pago = pago;
                    return Page();
                }

                // Bitácora con motivo
                await _bitacoraApi.RegistrarAsync(
                    userId.Value,
                    $"Reversó el pago {IdPago} (Factura {pago.ID_Factura}). Motivo: {Motivo}");

                TempData["ErrorMessage"] = "Pago reversado correctamente.";
                return RedirectToPage("./Index");
            }
            catch
            {
                ErrorMessage = "Ocurrió un error al reversar el pago.";
                return Page();
            }
        }
    }
}
