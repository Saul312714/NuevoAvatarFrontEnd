using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Helpers;

namespace NuevoAvatarFront.Services
{
    public interface ICarreraService
    {
        Task<List<CarreraDto>> GetAllAsync();
        Task<CarreraDto?> GetByIdAsync(int id);
    }

    public class CarreraService : ApiClientBase, ICarreraService
    {
        private readonly HttpClient _http;
        private readonly ILogger<CarreraService> _logger;

        public CarreraService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            TokenHelper tokenHelper,
            ILogger<CarreraService> logger
        ) : base(httpContextAccessor, tokenHelper)
        {
            _http = httpClientFactory.CreateClient("OfertaClient");
            _logger = logger;
        }

        public async Task<List<CarreraDto>> GetAllAsync()
        {
            try
            {
                await AttachBearerAsync(_http);

                using var resp = await _http.GetAsync("/carrera");

                if (!resp.IsSuccessStatusCode)
                {
                    var error = await resp.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "GET /carrera falló. Status: {Status} - Error: {Error}",
                        resp.StatusCode, error
                    );
                    return new List<CarreraDto>();
                }

                var data = await resp.Content.ReadFromJsonAsync<List<CarreraDto>>();
                return data ?? new List<CarreraDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado llamando a GET /carrera");
                throw;
            }
        }

        public async Task<CarreraDto?> GetByIdAsync(int id)
        {
            try
            {
                
                await AttachBearerAsync(_http);

                using var resp = await _http.GetAsync($"/carrera/{id}");

                if (resp.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (!resp.IsSuccessStatusCode)
                {
                    var error = await resp.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "GET /carrera/{Id} falló. Status: {Status} - Error: {Error}",
                        id, resp.StatusCode, error
                    );
                    throw new Exception("Error al obtener la carrera.");
                }

                return await resp.Content.ReadFromJsonAsync<CarreraDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado llamando a GET /carrera/{Id}", id);
                throw;
            }
        }
    }
}
