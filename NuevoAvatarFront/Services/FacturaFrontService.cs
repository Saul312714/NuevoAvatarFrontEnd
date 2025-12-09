using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;

namespace NuevoAvatarFront.Services
{
    public interface IFacturaFrontService
    {
        Task<List<FacturaDto>> ListarAsync(int? idPeriodo, int? idEstudiante, string? estado);

        Task<FacturaDto?> ObtenerPorIdAsync(int id);
        Task<int> CrearAsync(int idEstudiante, int idMatricula, int idPeriodo);

        Task ReversarAsync(int idFactura, string motivo);
    }

    public class FacturaFrontService : ApiClientBase, IFacturaFrontService
    {
        private readonly HttpClient _http;
        private readonly ILogger<FacturaFrontService> _logger;

        public FacturaFrontService(
            IHttpClientFactory factory,
            IHttpContextAccessor httpContextAccessor,
            TokenHelper tokenHelper,
            ILogger<FacturaFrontService> logger
        ) : base(httpContextAccessor, tokenHelper)
        {
            _http = factory.CreateClient("PagosClient");
            _logger = logger;
        }

        private int ObtenerIdUsuario()
        {
            var id = _httpContextAccessor.HttpContext?.Session.GetInt32("UsuarioID") ?? 0;

            if (id == 0)
            {
                _logger.LogWarning("⚠ UsuarioID no encontrado en sesión, posible sesión expirada.");
            }

            return id;
        }

        public async Task<List<FacturaDto>> ListarAsync(
            int? idPeriodo,
            int? idEstudiante,
            string? estado)
        {
            await AttachBearerAsync(_http);

            var qs = new List<string>();

            if (idPeriodo.HasValue)
                qs.Add($"idPeriodo={idPeriodo.Value}");

            if (idEstudiante.HasValue)
                qs.Add($"idEstudiante={idEstudiante.Value}");

            if (!string.IsNullOrWhiteSpace(estado))
                qs.Add($"estado={estado}");

            var url = "/factura";
            if (qs.Count > 0)
                url += "?" + string.Join("&", qs);

            var response = await _http.GetAsync(url);

            await EnsureSuccessOrThrowAsync(
                response,
                _logger,
                "No fue posible obtener el listado de facturas."
            );

            var lista = await response.Content.ReadFromJsonAsync<List<FacturaDto>>()
                        ?? new List<FacturaDto>();

            return lista;
        }

        public async Task<FacturaDto?> ObtenerPorIdAsync(int id)
        {
            await AttachBearerAsync(_http);

            var response = await _http.GetAsync($"/factura/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            await EnsureSuccessOrThrowAsync(
                response,
                _logger,
                "No fue posible obtener la factura."
            );

            var factura = await response.Content.ReadFromJsonAsync<FacturaDto>();
            return factura;
        }

        public async Task<int> CrearAsync(int idEstudiante, int idMatricula, int idPeriodo)
        {
            await AttachBearerAsync(_http);

            var idUsuario = ObtenerIdUsuario();

            var url = $"/factura" +
                      $"?idEstudiante={idEstudiante}" +
                      $"&idMatricula={idMatricula}" +
                      $"&idPeriodo={idPeriodo}" +
                      $"&idUsuario={idUsuario}";

            var response = await _http.PostAsync(url, null as HttpContent);

            await EnsureSuccessOrThrowAsync(
                response,
                _logger,
                "No fue posible crear la factura."
            );

            var factura = await response.Content.ReadFromJsonAsync<FacturaDto>();
            return factura?.ID_Factura ?? 0;
        }

        public async Task ReversarAsync(int idFactura, string motivo)
        {
            await AttachBearerAsync(_http);

            var idUsuario = ObtenerIdUsuario();

            var url = $"/factura/{idFactura}/reversar?idUsuario={idUsuario}";
            var payload = new { Motivo = motivo };

            var response = await _http.PostAsJsonAsync(url, payload);

            await EnsureSuccessOrThrowAsync(
                response,
                _logger,
                "No fue posible reversar la factura."
            );
        }
    }
}
