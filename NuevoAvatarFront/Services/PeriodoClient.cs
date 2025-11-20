using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;

namespace NuevoAvatarFront.Services
{
    public class PeriodoClient : ApiClientBase
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _cfg;
        private readonly ILogger<PeriodoClient> _logger;

        public PeriodoClient(
            HttpClient http,
            IHttpContextAccessor httpCtx,
            IConfiguration cfg,
            TokenHelper tokenHelper,
            ILogger<PeriodoClient> logger
        ) : base(httpCtx, tokenHelper)
        {
            _http = http;
            _cfg = cfg;
            _logger = logger;
        }

        private async Task AttachBearerAsync()
        {
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                token = _cfg["Auth:DevBearerToken"];
            }

            _http.DefaultRequestHeaders.Authorization =
                string.IsNullOrWhiteSpace(token)
                    ? null
                    : new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<Models.PeriodoDto>> GetAllAsync()
        {
            try
            {
                await AttachBearerAsync();

                using var resp = await _http.GetAsync("/periodo");

                await EnsureSuccessOrThrowAsync(
                    resp,
                    _logger,
                    "No fue posible obtener el listado de períodos. Intente nuevamente o contacte al administrador."
                );

                resp.EnsureSuccessStatusCode();

                return await resp.Content.ReadFromJsonAsync<List<Models.PeriodoDto>>() ?? new();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en GET /periodo");
                throw;
            }
        }

        public async Task<Models.PeriodoDto?> GetByIdAsync(int id)
        {
            try
            {
                await AttachBearerAsync();

                using var resp = await _http.GetAsync($"/periodo/{id}");

                if (resp.StatusCode == HttpStatusCode.NotFound)
                    return null;

                await EnsureSuccessOrThrowAsync(
                    resp,
                    _logger,
                    "No fue posible obtener la información del período."
                );

                if (!resp.IsSuccessStatusCode) return null;

                return await resp.Content.ReadFromJsonAsync<Models.PeriodoDto?>();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en GET /periodo/{Id}", id);
                throw;
            }
        }

        public async Task<bool> CreateAsync(Models.PeriodoUpsertDto dto)
        {
            try
            {
                await AttachBearerAsync();

                var body = new
                {
                    anio = dto.Anno,
                    numero_Periodo = dto.Numero,
                    fecha_Inicio = dto.Fecha_Inicio,
                    fecha_Fin = dto.Fecha_Fin
                };

                using var resp = await _http.PostAsJsonAsync("/periodo", body);

                await EnsureSuccessOrThrowAsync(
                    resp,
                    _logger,
                    "No fue posible crear el período. Verifique los datos ingresados."
                );

                if (!resp.IsSuccessStatusCode)
                {
                    var detail = await resp.Content.ReadAsStringAsync();
                    throw new Exception($"POST /periodo -> {(int)resp.StatusCode} {resp.StatusCode}. {detail}");
                }

                return true;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear período");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Models.PeriodoUpsertDto dto)
        {
            try
            {
                await AttachBearerAsync();

                var body = new
                {
                    anio = dto.Anno,
                    numero_Periodo = dto.Numero,
                    fecha_Inicio = dto.Fecha_Inicio,
                    fecha_Fin = dto.Fecha_Fin
                };

                using var resp = await _http.PutAsJsonAsync($"/periodo/{dto.ID_Periodo}", body);

                await EnsureSuccessOrThrowAsync(
                    resp,
                    _logger,
                    "No fue posible actualizar el período. Verifique los datos ingresados."
                );

                if (!resp.IsSuccessStatusCode)
                {
                    var detail = await resp.Content.ReadAsStringAsync();
                    throw new Exception(
                        $"PUT /periodo/{dto.ID_Periodo} -> {(int)resp.StatusCode} {resp.StatusCode}. {detail}"
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
                _logger.LogError(ex, "Error inesperado al actualizar período {Id}", dto.ID_Periodo);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                await AttachBearerAsync();

                using var resp = await _http.DeleteAsync($"/periodo/{id}");

                await EnsureSuccessOrThrowAsync(
                    resp,
                    _logger,
                    "No fue posible eliminar el período. Verifique que no esté siendo utilizado en otras operaciones."
                );

                return resp.IsSuccessStatusCode;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar período {Id}", id);
                throw;
            }
        }
    }
}
