using NuevoAvatarFront.Entities;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace NuevoAvatarFront.Services
{
    public interface IAuthApiClient
    {
        Task<(LoginResponse? Data, ApiError? Error)> LoginAsync(string email, string password);


        Task<(LoginResponse? Data, ApiError? Error)> RefreshAsync(string refreshToken);

    }

    public class AuthApiClient : IAuthApiClient
    {
        private readonly HttpClient _http;

        public AuthApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<(LoginResponse? Data, ApiError? Error)> LoginAsync(string email, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "Login/login");

            request.Headers.Add("usuario", email);
            request.Headers.Add("contrasena", password);
            request.Content = new StringContent("");

            var response = await _http.SendAsync(request);
            var rawContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return (data, null);
            }

            var error = new ApiError
            {
                StatusCode = (int)response.StatusCode,
                Message = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.BadRequest => "Solicitud incorrecta. Faltan datos.",
                    System.Net.HttpStatusCode.Unauthorized => "Usuario y/o contraseña incorrectos.",
                    System.Net.HttpStatusCode.Forbidden => "No tiene permisos.",
                    System.Net.HttpStatusCode.NotFound => "El recurso no existe.",
                    System.Net.HttpStatusCode.InternalServerError => "Error interno del servidor.",
                    _ => "Error desconocido."
                },
                Details = rawContent
            }; 

            return (null, error);
        }


        public async Task<(LoginResponse? Data, ApiError? Error)> RefreshAsync(string refreshToken)
        {
            var response = await _http.PostAsJsonAsync("Login/refresh", new { refreshToken });
            var rawContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return (data, null);
            }

            var error = new ApiError
            {
                StatusCode = (int)response.StatusCode,
                Message = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "Token inválido o expirado."
                    : "Error en el proceso de refresh.",
                Details = rawContent
            };

            return (null, error);
        }
    }

}

