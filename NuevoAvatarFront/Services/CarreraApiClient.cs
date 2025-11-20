using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.DTOs;

namespace NuevoAvatarFront.Services
{
    public class CarreraApiClient : ICarreraApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _contextAccessor;

        public CarreraApiClient(HttpClient http, IHttpContextAccessor contextAccessor)
        {
            _http = http;
            _contextAccessor = contextAccessor;
        }

        private void AddAuthorizationHeader()
        {
            var token = _contextAccessor.HttpContext?.Session.GetString("AccessToken");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // 
        // GET todas carreras
        // 
        public async Task<List<CarreraDto>> GetAllAsync()
        {
            AddAuthorizationHeader();
            string url = "carrera";

            var res = await _http.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error API Carrera GetAll: {await res.Content.ReadAsStringAsync()}");
                return new List<CarreraDto>();
            }

            return await res.Content.ReadFromJsonAsync<List<CarreraDto>>()
                   ?? new List<CarreraDto>();
        }

        // 
        // GET carrera por Institución
        // 
        public async Task<List<CarreraDto>> GetByInstitucionAsync(int idInstitucion)
        {
            AddAuthorizationHeader();
            string url = $"carrera/institucion/{idInstitucion}";

            var res = await _http.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error API Carrera GetByInstitucion: {await res.Content.ReadAsStringAsync()}");
                return new List<CarreraDto>();
            }

            return await res.Content.ReadFromJsonAsync<List<CarreraDto>>()
                   ?? new List<CarreraDto>();
        }

        // 
        // GET por ID
        // 
        public async Task<CarreraDto?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();
            string url = $"carrera/{id}";

            var res = await _http.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error API Carrera GetById: {await res.Content.ReadAsStringAsync()}");
                return null;
            }

            return await res.Content.ReadFromJsonAsync<CarreraDto>();
        }

        // 
        // CREATE
        // 
        public async Task<bool> CreateAsync(CarreraCreateDto dto)
        {
            AddAuthorizationHeader();
            string url = $"carrera";

            var res = await _http.PostAsJsonAsync(url, dto);

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error API Carrera Create: {await res.Content.ReadAsStringAsync()}");
                return false;
            }

            return true;
        }

        // 
        // UPDATE
        // 
        public async Task<bool> UpdateAsync(int id, CarreraUpdateDto dto)
        {
            AddAuthorizationHeader();
            string url = $"carrera/{id}";

            var res = await _http.PutAsJsonAsync(url, dto);

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error API Carrera Update: {await res.Content.ReadAsStringAsync()}");
                return false;
            }

            return true;
        }

        //
        // DELETE
        // 
        public async Task<bool> DeleteAsync(int id)
        {
            AddAuthorizationHeader();
            string url = $"carrera/{id}";

            var res = await _http.DeleteAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error API Carrera Delete: {await res.Content.ReadAsStringAsync()}");
                return false;
            }

            return true;
        }
    }
}
