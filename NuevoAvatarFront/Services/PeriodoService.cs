using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;

namespace NuevoAvatarFront.Services
{
    public interface IPeriodoService
    {
        Task<List<PeriodoDto>> GetPeriodosFuturosAsync();
    }

    public class PeriodoService : ApiClientBase, IPeriodoService
    {
        private readonly HttpClient _http;
        private readonly ILogger<PeriodoService> _logger;

        public PeriodoService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PeriodoService> logger
        ) : base(httpContextAccessor)
        {
           
            _http = httpClientFactory.CreateClient("OfertaClient");
            _logger = logger;
        }

        public async Task<List<PeriodoDto>> GetPeriodosFuturosAsync()
        {
            try
            {
                AttachBearer(_http); 

                
                var resp = await _http.GetAsync("/periodo");

                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    _logger.LogError("Error al obtener periodos. Status: {Status}, Error: {Error}",
                        resp.StatusCode, err);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al obtener periodos futuros");
                return new List<PeriodoDto>();
            }
        }
    }
}
