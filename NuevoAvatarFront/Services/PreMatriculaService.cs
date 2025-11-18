using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;

namespace NuevoAvatarFront.Services
{
    public interface IPreMatriculaService
    {
        Task<List<PreMatriculaDTO>> GetAllAsync();
        Task<PreMatriculaDTO?> GetByIdAsync(int id);
        Task<PreMatriculaDTO> CreateAsync(PreMatriculaDTO prematricula);
        Task<PreMatriculaDTO> UpdateAsync(int id, PreMatriculaDTO prematricula);
        Task<bool> DeleteAsync(int id);
    }

    public class PreMatriculaService : ApiClientBase, IPreMatriculaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PreMatriculaService> _logger;

        public PreMatriculaService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            TokenHelper tokenHelper,
            ILogger<PreMatriculaService> logger
        ) : base(httpContextAccessor, tokenHelper)
        {
            _httpClient = httpClientFactory.CreateClient("MatriculaClient");
            _configuration = configuration;
            _logger = logger;
        }

        private int ObtenerIdUsuario()
        {
            var userId = _configuration["Auth:DevUserId"];
            return int.TryParse(userId, out var id) ? id : 0;
        }

        public async Task<List<PreMatriculaDTO>> GetAllAsync()
        {
            try
            {
                await AttachBearerAsync(_httpClient);
                var idUsuario = ObtenerIdUsuario();

                var response =
                    await _httpClient.GetAsync($"/prematricula?idUsuario={idUsuario}");

                if (response.IsSuccessStatusCode)
                {
                    var prematriculas =
                        await response.Content.ReadFromJsonAsync<List<PreMatriculaDTO>>();
                    return prematriculas ?? new List<PreMatriculaDTO>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    $"Error al obtener prematrículas. Status: {response.StatusCode}, Error: {errorContent}");

                return new List<PreMatriculaDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener prematrículas");
                throw;
            }
        }

        public async Task<PreMatriculaDTO?> GetByIdAsync(int id)
        {
            try
            {
                await AttachBearerAsync(_httpClient);
                var idUsuario = ObtenerIdUsuario();

                var response =
                    await _httpClient.GetAsync($"/prematricula/{id}?idUsuario={idUsuario}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PreMatriculaDTO>();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    $"Error al obtener prematrícula {id}. Status: {response.StatusCode}, Error: {errorContent}");

                throw new Exception($"Error al obtener la prematrícula: {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener prematrícula {id}");
                throw;
            }
        }

        public async Task<PreMatriculaDTO> CreateAsync(PreMatriculaDTO prematricula)
        {
            try
            {
                await AttachBearerAsync(_httpClient);
                var idUsuario = ObtenerIdUsuario();

                var json = JsonSerializer.Serialize(prematricula);
                var content =
                    new StringContent(json, Encoding.UTF8, "application/json");

                var response =
                    await _httpClient.PostAsync($"/prematricula?idUsuario={idUsuario}", content);

                if (response.IsSuccessStatusCode)
                {
                    var creada =
                        await response.Content.ReadFromJsonAsync<PreMatriculaDTO>();
                    return creada ?? prematricula;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    $"Error al crear prematrícula. Status: {response.StatusCode}, Error: {errorContent}");

                if (errorContent.Contains("periodos FUTUROS"))
                {
                    throw new Exception(
                        "Solo se pueden prematricular periodos futuros (fecha de inicio posterior a la actual)");
                }

                throw new Exception($"Error al crear la prematrícula: {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear prematrícula");
                throw;
            }
        }

        public async Task<PreMatriculaDTO> UpdateAsync(int id, PreMatriculaDTO prematricula)
        {
            try
            {
                await AttachBearerAsync(_httpClient);
                var idUsuario = ObtenerIdUsuario();

                prematricula.ID_Prematricula = id;

                var json = JsonSerializer.Serialize(prematricula);
                var content =
                    new StringContent(json, Encoding.UTF8, "application/json");

                var response =
                    await _httpClient.PutAsync($"/prematricula/{id}?idUsuario={idUsuario}", content);

                if (response.IsSuccessStatusCode)
                {
                    var actualizada =
                        await response.Content.ReadFromJsonAsync<PreMatriculaDTO>();
                    return actualizada ?? prematricula;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    $"Error al actualizar prematrícula {id}. Status: {response.StatusCode}, Error: {errorContent}");

                if (errorContent.Contains("periodos FUTUROS"))
                {
                    throw new Exception(
                        "Solo se pueden prematricular periodos futuros (fecha de inicio posterior a la actual)");
                }

                throw new Exception($"Error al actualizar la prematrícula: {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar prematrícula {id}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                await AttachBearerAsync(_httpClient);
                var idUsuario = ObtenerIdUsuario();

                var response =
                    await _httpClient.DeleteAsync($"/prematricula/{id}?idUsuario={idUsuario}");

                if (response.IsSuccessStatusCode)
                    return true;

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    $"Error al eliminar prematrícula {id}. Status: {response.StatusCode}, Error: {errorContent}");

                throw new Exception($"Error al eliminar la prematrícula: {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar prematrícula {id}");
                throw;
            }
        }
    }
}

