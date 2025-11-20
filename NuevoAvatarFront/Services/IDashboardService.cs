
using NuevoAvatarFront.Entities.Dashboard;

namespace NuevoAvatarFront.Services

{
    public interface IDashboardService
    {
        Task<DashboardData> LoadDashboard(int idUsuario, string rolUsuario, string token);
    }
}
