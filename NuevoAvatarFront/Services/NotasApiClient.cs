using Microsoft.AspNetCore.Http;
using NuevoAvatarFront.Entities;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NuevoAvatarFront.Services
{
    public class NotasApiClient : INotasApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _contextAccessor;

        public NotasApiClient(HttpClient http, IHttpContextAccessor contextAccessor)
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

        //  OBTENER DESGLOSE (rubros de un grupo)
        public async Task<List<NotaRubro>> ObtenerDesgloseAsync(int idGrupo, int idUsuario)
        {
            AddAuthorizationHeader();

            var url = $"notas/obtenerdesglose/{idGrupo}?idUsuario={idUsuario}";
            var res = await _http.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error API Notas.ObtenerDesglose: {await res.Content.ReadAsStringAsync()}");
                return new List<NotaRubro>();
            }

            return await res.Content.ReadFromJsonAsync<List<NotaRubro>>()
                   ?? new List<NotaRubro>();
        }

        //  CARGAR DESGLOSE (guardar rubros)
        public async Task<(bool Ok, string Msg, List<NotaRubro>? Data)> CargarDesgloseAsync(
            int idGrupo,
            List<NotaRubro> rubros,
            int idUsuario)
        {
            AddAuthorizationHeader();

            var url = $"notas/cargardesglose?idGrupo={idGrupo}&idUsuario={idUsuario}";
            var res = await _http.PostAsJsonAsync(url, rubros);

            if (!res.IsSuccessStatusCode)
            {
                var msg = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error API Notas.CargarDesglose: {msg}");
                return (false, msg, null);
            }

            var wrapper = await res.Content.ReadFromJsonAsync<NotasWrapper<List<NotaRubro>>>();

            return (true, wrapper?.message ?? "Desglose guardado.", wrapper?.data);
        }

        //  OBTENER NOTAS DE UN ESTUDIANTE
        public async Task<List<Nota>> ObtenerNotasAsync(int idEstudiante, int idGrupo, int idUsuario)
        {
            AddAuthorizationHeader();

            var url = $"notas/obtenernotas?idEstudiante={idEstudiante}&idGrupo={idGrupo}&idUsuario={idUsuario}";
            var res = await _http.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error API Notas.ObtenerNotas: {await res.Content.ReadAsStringAsync()}");
                return new List<Nota>();
            }

            return await res.Content.ReadFromJsonAsync<List<Nota>>()
                   ?? new List<Nota>();
        }

        //  ASIGNAR / ACTUALIZAR NOTA
        public async Task<(bool Ok, string Msg, List<Nota>? Data)> AsignarNotaAsync(Nota nota, int idUsuario)
        {
            AddAuthorizationHeader();

            var url = $"notas/asignarnotarubro?idUsuario={idUsuario}";
            var res = await _http.PostAsJsonAsync(url, nota);

            if (!res.IsSuccessStatusCode)
            {
                var msg = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error API Notas.AsignarNota: {msg}");
                return (false, msg, null);
            }

            var wrapper = await res.Content.ReadFromJsonAsync<NotasWrapper<List<Nota>>>();

            return (true, wrapper?.message ?? "Nota guardada.", wrapper?.data);
        }
    }

    //  WRAPPER GENÉRICO PARA RESPUESTAS DEL BACKEND (message + data)
    public class NotasWrapper<T>
    {
        public string message { get; set; } = string.Empty;
        public T? data { get; set; }
    }
}
