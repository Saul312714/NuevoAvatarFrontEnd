using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public interface IBitacoraApiClient
    {
        // Registrar (ya lo usabas para login/logout)
        Task RegistrarAsync(int idUsuarioAccion, string descripcion, string? modulo = null, string? accion = null,
            string? jsonAntes = null, string? jsonDespues = null);

        // Listar todas las bitácoras
        Task<(bool ok, HttpStatusCode status, List<BitacoraDto>? Data, string? Message)>
            ListarAsync();
    }

    public class BitacoraApiClient : IBitacoraApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _context;

        public BitacoraApiClient(HttpClient http, IHttpContextAccessor context)
        {
            _http = http;
            _context = context;
        }

        public async Task RegistrarAsync(int idUsuario, string accion)
        {
            try
            {
                var data = new { ID_Usuario = idUsuario, Accion = accion };
                await _http.PostAsJsonAsync("bitacora", data);
            }
            catch
            {
                Console.WriteLine("No se pudo registrar la bitácora (frontend).");
            }
        }


        private void AddAuthHeader()
        {
            var token = _context.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
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

            if (raw.TrimStart().StartsWith("<")) // HTML = error interno
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

        private async Task<string> MapError(HttpResponseMessage response)
        {
            var raw = await response.Content.ReadAsStringAsync();
            string backendMessage = "";

            try
            {
                var payload = JsonSerializer.Deserialize<Dictionary<string, string?>>(raw);

                if (payload != null &&
                    payload.TryGetValue("error", out string? msg) &&
                    !string.IsNullOrWhiteSpace(msg))
                {
                    backendMessage = msg;
                }
            }
            catch { }

            return response.StatusCode switch
            {
                HttpStatusCode.BadRequest =>
                    backendMessage != "" ? backendMessage : "La información enviada no es válida.",

                HttpStatusCode.Unauthorized =>
                    "No tienes autorización.",

                HttpStatusCode.Forbidden =>
                    "No tienes permisos para realizar esta acción.",

                HttpStatusCode.NotFound =>
                    backendMessage != "" ? backendMessage : "No se encontraron registros de bitácora.",

                HttpStatusCode.InternalServerError =>
                    "Error interno del servidor. Inténtalo más tarde.",

                _ =>
                    backendMessage != "" ? backendMessage : "Ocurrió un error desconocido."
            };
        }

        // ======================================================
        // POST /bitacora  -> registrar
        // ======================================================
        public async Task RegistrarAsync(
            int idUsuarioAccion,
            string descripcion,
            string? modulo = null,
            string? accion = null,
            string? jsonAntes = null,
            string? jsonDespues = null)
        {
            AddAuthHeader();

            var bitacora = new
            {
                ID_Usuario = idUsuarioAccion,
                Descripcion = descripcion,
                Modulo = modulo,
                Accion = accion,
                JsonAntes = jsonAntes,
                JsonDespues = jsonDespues
            };

            await _http.PostAsJsonAsync("bitacora", bitacora);
        }

        // ======================================================
        // GET /bitacora  -> listar todas
        // ======================================================
        public async Task<(bool ok, HttpStatusCode status, List<BitacoraDto>? Data, string? Message)>
            ListarAsync()
        {
            AddAuthHeader();

            var res = await _http.GetAsync("bitacora");

            if (!res.IsSuccessStatusCode)
                return (false, res.StatusCode, null, await MapError(res));

            var data = await SafeJson<List<BitacoraDto>>(res);
            return (true, res.StatusCode, data ?? new List<BitacoraDto>(), null);
        }
    }
}
