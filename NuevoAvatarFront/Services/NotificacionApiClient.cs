using Microsoft.Extensions.Configuration;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Entities.Dashboard;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NuevoAvatarFront.Services
{
    public class NotificacionApiClient : INotificacionApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public NotificacionApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        private void AddToken(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private string BaseUrl =>
            _config["ApiNotificaciones"]
            ?? throw new InvalidOperationException("ApiNotificaciones no configurado.");

        public async Task<List<NotificacionDto>> GetAllAsync(string token)
        {
            AddToken(token);
            return await _http.GetFromJsonAsync<List<NotificacionDto>>($"{BaseUrl}/notificar")
                   ?? new List<NotificacionDto>();
        }

        public async Task<(bool ok, string? error)> EnviarAsync(string email, string asunto, string cuerpo, int idUsuario, string token)
        {
            AddToken(token);

            var url = $"{BaseUrl}/notificar?email={email}&asunto={asunto}&cuerpo={cuerpo}&idUsuario={idUsuario}";
            var response = await _http.PostAsync(url, null);

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, await response.Content.ReadAsStringAsync());
        }
    }
}
