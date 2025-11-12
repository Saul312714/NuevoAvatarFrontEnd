using NuevoAvatarFront.Entities;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NuevoAvatarFront.Services
{
    public interface IUsuarioApiClient
    {
        Task<UsuarioDto?> GetByIdAsync(int id, int idUsuarioAccion);
    }

    public class UsuarioApiClient : IUsuarioApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _contextAccessor;

        public UsuarioApiClient(HttpClient http, IHttpContextAccessor contextAccessor)
        {
            _http = http;
            _contextAccessor = contextAccessor;
        }

        public async Task<UsuarioDto?> GetByIdAsync(int id, int idUsuarioAccion)
        {
            //Obtener el token almacenado en la sesión
            var token = _contextAccessor.HttpContext?.Session.GetString("AccessToken");

            //Si hay token, agregarlo al header Authorization
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Construir la URL completa
            var url = $"usuario/{id}?idUsuarioAccion={idUsuarioAccion}";

            //Consumir el endpoint
            var response = await _http.GetAsync(url);

            // Si la API devuelve 401 o error, manejarlo con null
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UsuarioDto>();
        }
    }
}
