using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public class PagoApiClient : IPagoApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public PagoApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        private void AddToken(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private string BasePagos =>
            _config["ApiPagos"]
            ?? throw new InvalidOperationException("ApiPagos no configurado en appsettings.json");

        public async Task<List<PagoDto>> GetAllPagosAsync(string token)
        {
            AddToken(token);

            var result = await _http.GetFromJsonAsync<List<PagoDto>>($"{BasePagos}/pago");
            return result ?? new List<PagoDto>();
        }

        public async Task<PagoDto?> GetPagoByIdAsync(int idPago, string token)
        {
            AddToken(token);
            return await _http.GetFromJsonAsync<PagoDto>($"{BasePagos}/pago/{idPago}");
        }

        public async Task<(bool ok, string? error)> RegistrarPagoAsync(
            int idFactura,
            decimal monto,
            int idUsuario,
            string token)
        {
            AddToken(token);

            // El endpoint recibe idFactura, monto e idUsuario por querystring
            var montoStr = monto.ToString(CultureInfo.InvariantCulture);

            var url =
                $"{BasePagos}/pago?idFactura={idFactura}&monto={montoStr}&idUsuario={idUsuario}";

            var response = await _http.PostAsync(url, new StringContent(string.Empty));

            if (response.IsSuccessStatusCode)
                return (true, null);

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        public async Task<(bool ok, string? error)> ReversarPagoAsync(
            int idPago,
            int idUsuario,
            string token)
        {
            AddToken(token);

            var url = $"{BasePagos}/pago/{idPago}/reversar?idUsuario={idUsuario}";
            var response = await _http.PostAsync(url, new StringContent(string.Empty));

            if (response.IsSuccessStatusCode)
                return (true, null);

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        public async Task<List<FacturaDto>> GetFacturasAsync(string token)
        {
            AddToken(token);

            var result = await _http.GetFromJsonAsync<List<FacturaDto>>($"{BasePagos}/factura");
            return result ?? new List<FacturaDto>();
        }

        public async Task<FacturaDto?> GetFacturaByIdAsync(int idFactura, string token)
        {
            AddToken(token);
            return await _http.GetFromJsonAsync<FacturaDto>($"{BasePagos}/factura/{idFactura}");
        }
    }
}
