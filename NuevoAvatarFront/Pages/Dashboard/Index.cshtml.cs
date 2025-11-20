using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities.Dashboard;
using NuevoAvatarFront.Services;
using NuevoAvatarFront.Services.Dashboard;

namespace NuevoAvatarFront.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly IDashboardService _dashboard;

        public DashboardData Data { get; set; } = new();

        public IndexModel(IDashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        public async Task OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var idUsuario = HttpContext.Session.GetInt32("UsuarioID");
            var rol = HttpContext.Session.GetString("UserRole");
            var userName = HttpContext.Session.GetString("UserName");

            if (token is null || idUsuario is null)
            {
                Response.Redirect("/HU_MICHEL/Login");
                return;
            }

            Data = await _dashboard.LoadDashboard(idUsuario.Value, rol!, token);
            Data.NombreUsuario = userName ?? "Usuario";
        }
    }
}
