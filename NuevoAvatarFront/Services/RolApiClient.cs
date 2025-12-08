using NuevoAvatarFront.Entities;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace NuevoAvatarFront.Services
{
    public class RolApiClient : IRolApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _context;

        public RolApiClient(HttpClient http, IHttpContextAccessor context)
        {
            _http = http;
            _context = context;
        }

        // ===========================
        // Helpers
        // ===========================
        private void AddAuthHeader()
        {
            var token = _context.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private async Task<T?> SafeJson<T>(HttpResponseMessage res)
        {
            var raw = await res.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(raw))
                return default;

            // Si el backend se cae y devuelve HTML
            if (raw.TrimStart().StartsWith("<"))
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(raw, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return default;
            }
        }

        private async Task<string?> BuildFriendlyError(HttpResponseMessage response)
        {
            var raw = await response.Content.ReadAsStringAsync();

            // 1) Intentar leer { error: "..."} del backend
            if (!string.IsNullOrWhiteSpace(raw) && !raw.TrimStart().StartsWith("<"))
            {
                try
                {
                    var payload = JsonSerializer.Deserialize<Dictionary<string, string?>>(raw,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (payload != null &&
                        payload.TryGetValue("error", out var backendMsg) &&
                        !string.IsNullOrWhiteSpace(backendMsg))
                    {
                        // Usamos directamente el mensaje del backend
                        return backendMsg;
                    }
                }
                catch
                {
                    // si falla el parseo, sigo al switch
                }
            }

            // 2) Fallback: traducir por código de estado
            return response.StatusCode switch
            {
                HttpStatusCode.BadRequest => "La información enviada no es válida.",
                HttpStatusCode.NotFound => "El rol indicado no existe.",
                HttpStatusCode.Conflict => "No se puede completar la acción por un conflicto de datos.",
                HttpStatusCode.Unauthorized => "Debe iniciar sesión nuevamente.",
                HttpStatusCode.Forbidden => "No tiene permisos para realizar esta operación.",
                HttpStatusCode.InternalServerError => "Ocurrió un error interno en el servidor.",
                _ => response.ReasonPhrase ?? "Ocurrió un error al procesar la solicitud."
            };
        }

        // ===========================
        // GET ALL
        // ===========================
        public async Task<(bool ok, HttpStatusCode status, List<RolDto>? Data, string? Message)>
            GetAllAsync(int idUsuarioAccion)
        {
            AddAuthHeader();

            var res = await _http.GetAsync($"rol?idUsuarioAccion={idUsuarioAccion}");

            if (!res.IsSuccessStatusCode)
                return (false, res.StatusCode, null, await BuildFriendlyError(res));

            var data = await SafeJson<List<RolDto>>(res);
            return (true, res.StatusCode, data, null);
        }

        // ===========================
        // GET BY ID
        // ===========================
        public async Task<(bool ok, HttpStatusCode status, RolDto? Data, string? Message)>
            GetByIdAsync(int id, int idUsuarioAccion)
        {
            AddAuthHeader();

            var res = await _http.GetAsync($"/rol/{id}?idUsuarioAccion={idUsuarioAccion}");

            if (!res.IsSuccessStatusCode)
                return (false, res.StatusCode, null, await BuildFriendlyError(res));

            var data = await SafeJson<RolDto>(res);
            return (true, res.StatusCode, data, null);
        }

        // ===========================
        // CREATE
        // ===========================
        public async Task<(bool ok, HttpStatusCode status, RolDto? Data, string? Message)>
            CreateAsync(RolDto rol, int idUsuarioAccion)
        {
            AddAuthHeader();

            var res = await _http.PostAsJsonAsync($"/rol?idUsuarioAccion={idUsuarioAccion}", rol);

            if (!res.IsSuccessStatusCode)
                return (false, res.StatusCode, null, await BuildFriendlyError(res));

            var data = await SafeJson<RolDto>(res);
            return (true, res.StatusCode, data, null);
        }

        // ===========================
        // UPDATE
        // ===========================
        public async Task<(bool ok, HttpStatusCode status, RolDto? Data, string? Message)>
            UpdateAsync(RolDto rol, int idUsuarioAccion)
        {
            AddAuthHeader();

            var res = await _http.PutAsJsonAsync(
                $"/rol/{rol.ID_Rol}?idUsuarioAccion={idUsuarioAccion}", rol);

            if (!res.IsSuccessStatusCode)
                return (false, res.StatusCode, null, await BuildFriendlyError(res));

            var data = await SafeJson<RolDto>(res);
            return (true, res.StatusCode, data, null);
        }

        // ===========================
        // DELETE
        // ===========================
        public async Task<(bool ok, HttpStatusCode status, string? Message)>
            DeleteAsync(int id, int idUsuarioAccion)
        {
            AddAuthHeader();

            var res = await _http.DeleteAsync($"/rol/{id}?idUsuarioAccion={idUsuarioAccion}");

            if (!res.IsSuccessStatusCode)
                return (false, res.StatusCode, await BuildFriendlyError(res));

            return (true, res.StatusCode, null);
        }
    }
}
