using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;
using System.Text.Json;

namespace NuevoAvatarFront.Services
{
    public class ProfesorClient : ApiClientBase
    {
        private readonly HttpClient _http;
        private readonly ILogger<ProfesorClient> _logger;
        private readonly IConfiguration _cfg;

        public ProfesorClient(
            HttpClient http,
            ILogger<ProfesorClient> logger,
            IHttpContextAccessor httpCtx,
            IConfiguration cfg,
            TokenHelper tokenHelper
        ) : base(httpCtx, tokenHelper)
        {
            _http = http;
            _logger = logger;
            _cfg = cfg;
        }

        private async Task AttachBearerAsync()
        {
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
                token = _cfg["Auth:DevBearerToken"];

            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _http.DefaultRequestHeaders.Authorization = null;
            }
        }

        private string MapBackendError(string rawBody, HttpStatusCode statusCode, string fallback)
        {
            if (string.IsNullOrWhiteSpace(rawBody))
                return fallback;

            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("Error", out var errProp) &&
                        errProp.ValueKind == JsonValueKind.String)
                    {
                        return errProp.GetString() ?? fallback;
                    }

                    if (root.TryGetProperty("Mensaje", out var msgProp) &&
                        msgProp.ValueKind == JsonValueKind.String)
                    {
                        return msgProp.GetString() ?? fallback;
                    }
                }
            }
            catch
            {
              
            }

            if (rawBody.Contains("grupos en períodos activos", StringComparison.OrdinalIgnoreCase))
            {
                return "No se puede eliminar el profesor porque dicta grupos en períodos activos.";
            }
            
            return fallback;
        }

        public async Task<(IEnumerable<ProfesorDto> datos, int total)> GetAllAsync(string? q, string? sort)
        {
            try
            {
                await AttachBearerAsync();

                var url = "/profesor";

                using var resp = await _http.GetAsync(url);

                await EnsureSuccessOrThrowAsync(
                    resp,
                    _logger,
                    "No fue posible obtener la lista de profesores. Intente nuevamente más tarde."
                );

                var list =
                    await resp.Content.ReadFromJsonAsync<List<ProfesorDto>>()
                    ?? new List<ProfesorDto>();

                return (list, list.Count);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener profesores");
                throw;
            }
        }

        public async Task<ProfesorDto?> GetByIdAsync(int id)
        {
            try
            {
                await AttachBearerAsync();

                using var resp = await _http.GetAsync($"/profesor/{id}");

                if (resp.StatusCode == HttpStatusCode.NotFound)
                    return null;

                await EnsureSuccessOrThrowAsync(
                    resp,
                    _logger,
                    "No fue posible obtener la información del profesor."
                );

                return await resp.Content.ReadFromJsonAsync<ProfesorDto>();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener profesor {Id}", id);
                throw;
            }
        }

        public async Task<bool> CreateAsync(ProfesorDto model)
        {
            try
            {
                await AttachBearerAsync();

                using var resp = await _http.PostAsJsonAsync("/profesor", model);

                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    _logger.LogWarning("Create profesor failed: {Status} Body: {Body}",
                        resp.StatusCode, body);

                    var mensajeUsuario = MapBackendError(
                        body,
                        resp.StatusCode,
                        "No fue posible registrar el profesor. Revise los datos e intente nuevamente."
                    );

                    IEnumerable<string>? errors = string.IsNullOrWhiteSpace(body)
                        ? null
                        : new[] { body };

                    throw new ApiException(
                        mensajeUsuario,
                        resp.StatusCode,
                        errors
                    );
                }

                return true;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear profesor");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(ProfesorDto model)
        {
            try
            {
                await AttachBearerAsync();

                using var resp = await _http.PutAsJsonAsync("/profesor", model);

                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    _logger.LogWarning("Update profesor failed: {Status} Body: {Body}",
                        resp.StatusCode, body);

                    var mensajeUsuario = MapBackendError(
                        body,
                        resp.StatusCode,
                        "No fue posible actualizar la información del profesor."
                    );

                    IEnumerable<string>? errors = string.IsNullOrWhiteSpace(body)
                        ? null
                        : new[] { body };

                    throw new ApiException(
                        mensajeUsuario,
                        resp.StatusCode,
                        errors
                    );
                }

                return true;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar profesor {Id}", model.ID_Profesor);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                await AttachBearerAsync();

                using var resp = await _http.DeleteAsync($"/profesor/{id}");

                if (resp.StatusCode == HttpStatusCode.NotFound)
                    return false;

                if (resp.IsSuccessStatusCode)
                    return true;

                var body = await resp.Content.ReadAsStringAsync();

                _logger.LogWarning(
                    "Delete profesor failed: {Status} Body: {Body}",
                    resp.StatusCode, body
                );

                string? backendMsg = null;
                IEnumerable<string>? errors = null;

                if (!string.IsNullOrWhiteSpace(body))
                {
                    errors = new[] { body };

                    try
                    {
                        using var doc = JsonDocument.Parse(body);
                        var root = doc.RootElement;

                        if (root.ValueKind == JsonValueKind.Object)
                        {
                            if (root.TryGetProperty("Error", out var p)) backendMsg = p.GetString();
                            else if (root.TryGetProperty("error", out p)) backendMsg = p.GetString();
                            else if (root.TryGetProperty("message", out p)) backendMsg = p.GetString();
                            else if (root.TryGetProperty("Message", out p)) backendMsg = p.GetString();
                        }
                    }
                    catch
                    {
                      
                    }
                }

                var message = backendMsg ??
                    "No fue posible eliminar el profesor. Intente nuevamente más tarde.";

                throw new ApiException(message, resp.StatusCode, errors);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar profesor {Id}", id);
                throw;
            }
        }
    }
}
