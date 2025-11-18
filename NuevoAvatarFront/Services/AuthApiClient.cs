using NuevoAvatarFront.Entities;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace NuevoAvatarFront.Services
{
    public interface IAuthApiClient
    {
        Task<LoginResponse?> LoginAsync(string email, string password);

        Task<LoginResponse?> RefreshAsync(string refreshToken);
    }


    public class AuthApiClient : IAuthApiClient
    {
        private readonly HttpClient _http;

        public AuthApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "Login/login");

            //Los encabezados que la API espera
            request.Headers.Add("usuario", email);
            request.Headers.Add("contrasena", password);

            request.Content = new StringContent(string.Empty);

            //Enviar la solicitud
            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            //Leer la respuesta con los tokens
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        public async Task<LoginResponse?> RefreshAsync(string refreshToken)
        {
            var response = await _http.PostAsJsonAsync("Login/refresh", new { refreshToken });
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }
    }
}

