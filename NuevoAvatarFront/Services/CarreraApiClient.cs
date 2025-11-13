using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NuevoAvatarFront.DTOs;

namespace NuevoAvatarFront.Services
{
    public class CarreraApiClient : ICarreraApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly string _baseUrl;

        public CarreraApiClient(HttpClient http, IConfiguration config, IHttpContextAccessor accessor)
        {
            _http = http;
            _contextAccessor = accessor;
            _baseUrl = config["ApiUrls:OfertaApi"]; // EJ: https://localhost:7217
        }

        
        private void AddAuthHeader()
        {
            var token = _contextAccessor.HttpContext?.Session.GetString("AccessToken");

            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<CarreraDto>> GetAllAsync()
        {
            AddAuthHeader();

            var result = await _http.GetAsync($"{_baseUrl}/carrera");

            if (!result.IsSuccessStatusCode)
                return new List<CarreraDto>();

            return await result.Content.ReadFromJsonAsync<List<CarreraDto>>()
                   ?? new List<CarreraDto>();
        }

        public async Task<List<CarreraDto>> GetByInstitucionAsync(int idInstitucion)
        {
            AddAuthHeader();

            var result = await _http.GetAsync($"{_baseUrl}/carrera/institucion/{idInstitucion}");

            if (!result.IsSuccessStatusCode)
                return new List<CarreraDto>();

            return await result.Content.ReadFromJsonAsync<List<CarreraDto>>()
                   ?? new List<CarreraDto>();
        }

        public async Task<CarreraDto?> GetByIdAsync(int id)
        {
            AddAuthHeader();

            var result = await _http.GetAsync($"{_baseUrl}/carrera/{id}");

            if (!result.IsSuccessStatusCode)
                return null;

            return await result.Content.ReadFromJsonAsync<CarreraDto>();
        }

        public async Task<bool> CreateAsync(CarreraDto carrera)
        {
            AddAuthHeader();

            var result = await _http.PostAsJsonAsync($"{_baseUrl}/carrera", carrera);

            return result.IsSuccessStatusCode;
        }

      
        public async Task<bool> UpdateAsync(CarreraDto carrera)
        {
            AddAuthHeader();

            var result = await _http.PutAsJsonAsync($"{_baseUrl}/carrera", carrera);

            return result.IsSuccessStatusCode;
        }


   
        public async Task<bool> DeleteAsync(int id)
        {
            AddAuthHeader();

            var result = await _http.DeleteAsync($"{_baseUrl}/carrera/{id}");

            return result.IsSuccessStatusCode;
        }
    }
}
