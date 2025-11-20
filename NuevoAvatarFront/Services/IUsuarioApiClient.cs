using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public interface IUsuarioApiClient
    {
        Task<(bool ok, HttpStatusCode status, List<UsuarioDto>? Data, string? Message)>
            GetAllAsync(string? filtro = null, string? rol = null, int page = 1, int pageSize = 10);

        Task<(bool ok, HttpStatusCode status, UsuarioDto? Data, string? Message)>
            GetByIdAsync(int id, int idUsuarioAccion);

        Task<(bool ok, HttpStatusCode status, UsuarioDto? Data, string? Message)>
            CreateAsync(UsuarioDto usuario, int idUsuarioAccion);

        Task<(bool ok, HttpStatusCode status, UsuarioDto? Data, string? Message)>
            UpdateAsync(UsuarioDto usuario, int idUsuarioAccion);

        Task<(bool ok, HttpStatusCode status, string? Message)>
            DeleteAsync(int id, int idUsuarioAccion);
    }

    public class UsuarioApiClient : IUsuarioApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _context;

        public UsuarioApiClient(HttpClient http, IHttpContextAccessor context)
        {
            _http = http;
            _context = context;
        }

        // ============================================================
        // AGREGA CABECERA AUTH
        // ============================================================
        private void AddAuthHeader()
        {
            var token = _context.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        // ============================================================
        // LECTOR SEGURO DE JSON
        // ============================================================
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

        // ============================================================
        // SWITCH DE ERRORES + MENSAJES DEL BACKEND
        // ============================================================
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
                    backendMessage = msg; // Mensaje exacto del backend
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
                    backendMessage != "" ? backendMessage : "No se encontró el usuario solicitado.",

                HttpStatusCode.Conflict =>
                    backendMessage != "" ? backendMessage : "Conflicto: algún dato ya existe.",

                HttpStatusCode.InternalServerError =>
                    "Error interno del servidor. Inténtalo más tarde.",

                _ =>
                    backendMessage != "" ? backendMessage : "Ocurrió un error desconocido."
            };
        }

        // ============================================================
        // GET ALL
        // ============================================================
        public async Task<(bool ok, HttpStatusCode status, List<UsuarioDto>? Data, string? Message)>
            GetAllAsync(string? filtro = null, string? rol = null, int page = 1, int pageSize = 10)
        {
            AddAuthHeader();

            var idUsuarioAccion = _context.HttpContext?.Session.GetInt32("UsuarioID") ?? 0;

            var url = $"usuario?idUsuarioAccion={idUsuarioAccion}&page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(filtro)) url += $"&filtro={filtro}";
            if (!string.IsNullOrEmpty(rol)) url += $"&rol={rol}";

            var res = await _http.GetAsync(url);

            if (!res.IsSuccessStatusCode)
                return (false, res.StatusCode, null, await MapError(res));

            var data = await SafeJson<List<UsuarioDto>>(res);
            return (true, res.StatusCode, data, null);
        }

        // ============================================================
        // GET BY ID
        // ============================================================
        public async Task<(bool ok, HttpStatusCode status, UsuarioDto? Data, string? Message)>
            GetByIdAsync(int id, int idUsuarioAccion)
        {
            AddAuthHeader();

            var res = await _http.GetAsync($"usuario/{id}?idUsuarioAccion={idUsuarioAccion}");

            if (!res.IsSuccessStatusCode)
                return (false, res.StatusCode, null, await MapError(res));

            var data = await SafeJson<UsuarioDto>(res);
            return (true, res.StatusCode, data, null);
        }

        // ============================================================
        // CREATE
        // ============================================================
        public async Task<(bool ok, HttpStatusCode status, UsuarioDto? Data, string? Message)>
            CreateAsync(UsuarioDto usuario, int idUsuarioAccion)
        {
            AddAuthHeader();

            var payload = new
            {
                email = usuario.Email,
                tipo_Identificacion = usuario.Tipo_Identificacion,
                identificacion = usuario.Identificacion,
                nombre = usuario.Nombre,
                contrasena = usuario.Contrasena,
                iD_Rol = usuario.ID_Rol,
                rolNombre = usuario.RolNombre
            };

            var res = await _http.PostAsJsonAsync(
                $"usuario?idUsuarioAccion={idUsuarioAccion}", payload);

            if (!res.IsSuccessStatusCode)
                return (false, res.StatusCode, null, await MapError(res));

            var data = await SafeJson<UsuarioDto>(res);
            return (true, res.StatusCode, data, null);
        }

        // ============================================================
        // UPDATE
        // ============================================================
        public async Task<(bool ok, HttpStatusCode status, UsuarioDto? Data, string? Message)>
            UpdateAsync(UsuarioDto usuario, int idUsuarioAccion)
        {
            AddAuthHeader();

            var payload = new
            {
                iD_Usuario = usuario.ID_Usuario,
                email = usuario.Email,
                tipo_Identificacion = usuario.Tipo_Identificacion,
                identificacion = usuario.Identificacion,
                nombre = usuario.Nombre,
                contrasena = usuario.Contrasena,
                iD_Rol = usuario.ID_Rol,
                rolNombre = usuario.RolNombre
            };

            var res = await _http.PutAsJsonAsync(
                $"usuario/{usuario.ID_Usuario}?idUsuarioAccion={idUsuarioAccion}", payload);

            if (!res.IsSuccessStatusCode)
                return (false, res.StatusCode, null, await MapError(res));

            var data = await SafeJson<UsuarioDto>(res);
            return (true, res.StatusCode, data, null);
        }

        // ============================================================
        // DELETE
        // ============================================================
        public async Task<(bool ok, HttpStatusCode status, string? Message)>
            DeleteAsync(int id, int idUsuarioAccion)
        {
            AddAuthHeader();

            var res = await _http.DeleteAsync(
                $"usuario/{id}?idUsuarioAccion={idUsuarioAccion}");

            if (!res.IsSuccessStatusCode)
                return (false, res.StatusCode, await MapError(res));

            return (true, res.StatusCode, null);
        }
    }
}
