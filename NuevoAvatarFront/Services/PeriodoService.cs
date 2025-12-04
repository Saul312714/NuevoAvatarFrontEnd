using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;

namespace NuevoAvatarFront.Services
{
    public interface IPeriodoService
    {
        Task<List<PeriodoDto>> GetPeriodosFuturosAsync();

        // 🔹 NUEVO: obtener todos los períodos (para pantallas que lo necesitan)
        Task<List<PeriodoDto>> GetAllAsync();
    }

    public class PeriodoService : ApiClientBase, IPeriodoService
    {
        private readonly HttpClient _http;
        private readonly ILogger<PeriodoService> _logger;

        public PeriodoService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            TokenHelper tokenHelper,
            ILogger<PeriodoService> logger
        ) : base(httpContextAccessor, tokenHelper)
        {
            _http = httpClientFactory.CreateClient("OfertaClient");
            _logger = logger;
        }

        // ===========================
        //  PERIODOS FUTUROS (YA TENÍAS)
        // ===========================
        public async Task<List<PeriodoDto>> GetPeriodosFuturosAsync()
        {
            try
            {
                await AttachBearerAsync(_http);

                var resp = await _http.GetAsync("/periodo");

                await EnsureSuccessOrThrowAsync(
                    resp,
                    _logger,
                    "No fue posible obtener la lista de períodos. Intente nuevamente más tarde."
                );

                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Error al obtener periodos. Status: {Status}, Error: {Error}",
                        resp.StatusCode, err
                    );
                    return new List<PeriodoDto>();
                }

                var opciones = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var lista = await resp.Content.ReadFromJsonAsync<List<PeriodoDto>>(opciones)
                            ?? new List<PeriodoDto>();

                var hoy = DateTime.Today;

                return lista
                    .Where(p => p.Fecha_Inicio.Date > hoy)
                    .OrderBy(p => p.Anno)
                    .ThenBy(p => p.Numero)
                    .ToList();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al obtener periodos futuros");
                throw;
            }
        }

        // ===========================
        //  NUEVO: GET ALL PERIODOS
        // ===========================
        public async Task<List<PeriodoDto>> GetAllAsync()
        {
            try
            {
                await AttachBearerAsync(_http);

                var resp = await _http.GetAsync("/periodo");

                await EnsureSuccessOrThrowAsync(
                    resp,
                    _logger,
                    "No fue posible obtener la lista de períodos. Intente nuevamente más tarde."
                );

                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Error al obtener periodos (GetAllAsync). Status: {Status}, Error: {Error}",
                        resp.StatusCode, err
                    );
                    return new List<PeriodoDto>();
                }

                var opciones = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var lista = await resp.Content.ReadFromJsonAsync<List<PeriodoDto>>(opciones)
                            ?? new List<PeriodoDto>();

                // Aquí NO filtramos por futuros, devolvemos todo
                return lista
                    .OrderBy(p => p.Anno)
                    .ThenBy(p => p.Numero)
                    .ToList();
            }
            catch (ApiException)
            {
                // Dejo pasar el ApiException para que la Razor Page lo maneje bonito
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al obtener todos los periodos");
                throw;
            }
        }
    }
}
