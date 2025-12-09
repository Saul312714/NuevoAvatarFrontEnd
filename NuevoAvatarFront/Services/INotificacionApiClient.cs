using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Entities.Dashboard;

namespace NuevoAvatarFront.Services
{
    public interface INotificacionApiClient
    {
        Task<List<NotificacionDto>> GetAllAsync(string token);
        Task<(bool ok, string? error)> EnviarAsync(string email, string asunto, string cuerpo, int idUsuario, string token);
    }
}
