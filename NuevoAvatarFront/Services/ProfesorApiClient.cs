using System.Net.Http.Headers;
using System.Net.Http.Json;
using NuevoAvatarFront.DTOs;

namespace NuevoAvatarFront.Services
{
    public interface IProfesorApiClient
    {
        Task<List<ProfesorDto>> GetAllAsync();
        Task<List<ProfesorDto>> SearchAsync(string term);
    }

    public class ProfesorApiClient : IProfesorApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _context;

        public ProfesorApiClient(HttpClient http, IHttpContextAccessor context)
        {
            _http = http;
            _context = context;
        }

        //  Token SIEMPRE actualizado antes de cada request
        private void AddToken()
        {
            var token = _context.HttpContext?.Session.GetString("AccessToken");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        //  Obtener usuario en sesión
        private int GetUsuarioAccion()
        {
            var id = _context.HttpContext?.Session.GetInt32("UsuarioID") ?? 0;

            if (id == 0)
                Console.WriteLine(" Advertencia: UsuarioID=0 en ProfesorApiClient (revisar login/sesión)");

            return id;
        }

        //  Buscar profesor por texto
        public async Task<List<ProfesorDto>> SearchAsync(string term)
        {
            AddToken();
            int idUsuarioAccion = GetUsuarioAccion();

            var url = $"profesor/search?q={Uri.EscapeDataString(term)}&idUsuarioAccion={idUsuarioAccion}";

            var resp = await _http.GetAsync(url);

            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error SearchProfesor: {await resp.Content.ReadAsStringAsync()}");
                return new();
            }

            return await resp.Content.ReadFromJsonAsync<List<ProfesorDto>>() ?? new();
        }

        public async Task<List<ProfesorDto>> GetAllAsync()
        {
                AddToken();
                    int idUsuarioAccion = GetUsuarioAccion();

                    var url = $"profesor/?idUsuarioAccion={idUsuarioAccion}";

                    var resp = await _http.GetAsync(url);

                if (!resp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Error GetAllProfesor: {await resp.Content.ReadAsStringAsync()}");
                    return new ();
                }

                return await resp.Content.ReadFromJsonAsync<List<ProfesorDto>>() ?? new ();
        }

    }
}
