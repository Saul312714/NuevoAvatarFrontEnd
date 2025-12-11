using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public interface IPagoApiClient
    {
        Task<List<PagoDto>> GetAllPagosAsync(string token);
        Task<PagoDto?> GetPagoByIdAsync(int idPago, string token);

        Task<(bool ok, string? error)> RegistrarPagoAsync(
            int idFactura,
            decimal monto,
            int idUsuario,
            string token);

        Task<(bool ok, string? error)> ReversarPagoAsync(
            int idPago,
            int idUsuario,
            string token);

        Task<List<FacturaDto>> GetFacturasAsync(string token);
        Task<FacturaDto?> GetFacturaByIdAsync(int idFactura, string token);
    }
}
