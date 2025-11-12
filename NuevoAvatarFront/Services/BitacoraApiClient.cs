using System.Net.Http.Json;

namespace NuevoAvatarFront.Services
{
    public interface IBitacoraApiClient
    {
        Task RegistrarAsync(int idUsuario, string accion);
    }

    public class BitacoraApiClient : IBitacoraApiClient
    {
        private readonly HttpClient _http;

        public BitacoraApiClient(HttpClient http)
        {
            _http = http;
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
    }
}
