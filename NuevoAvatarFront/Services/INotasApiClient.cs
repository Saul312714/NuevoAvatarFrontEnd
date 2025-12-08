using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public interface INotasApiClient
    {
        Task<List<NotaRubro>> ObtenerDesgloseAsync(int idGrupo, int idUsuario);
        Task<(bool Ok, string Msg, List<NotaRubro>? Data)> CargarDesgloseAsync(int idGrupo, List<NotaRubro> rubros, int idUsuario);

        Task<List<Nota>> ObtenerNotasAsync(int idEstudiante, int idGrupo, int idUsuario);
        Task<(bool Ok, string Msg, List<Nota>? Data)> AsignarNotaAsync(Nota nota, int idUsuario);
    }
}
