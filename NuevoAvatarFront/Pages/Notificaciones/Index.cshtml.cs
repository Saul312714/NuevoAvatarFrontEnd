using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Entities.Dashboard;
using NuevoAvatarFront.Services;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace NuevoAvatarFront.Pages.Notificaciones
{
    public class IndexModel : PageModel
    {
        private readonly INotificacionApiClient _notiApi;
        private readonly IBitacoraApiClient _bitacoraApi;

        public IndexModel(INotificacionApiClient api, IBitacoraApiClient bitacora)
        {
            _notiApi = api;
            _bitacoraApi = bitacora;
        }

        [BindProperty] public string Email { get; set; } = "";
        [BindProperty] public string Asunto { get; set; } = "";
        [BindProperty] public string Cuerpo { get; set; } = "";
        [BindProperty] public bool Confirmado { get; set; }

        public string? ErrorMessage { get; set; }
        public string? ResultadoEnvio { get; set; }
        public bool EnvioExitoso { get; set; }
        public bool MostrarConfirmacion { get; set; }

        public string? CuerpoSanitizado { get; set; }

        public List<NotificacionDto> Historial { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");
            if (token is null || userId is null) return Redirect("/HU_MICHEL/Login");

            Historial = await _notiApi.GetAllAsync(token);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");
            if (token is null || userId is null) return Redirect("/HU_MICHEL/Login");

            Historial = await _notiApi.GetAllAsync(token);

            // -----------------------
            // VALIDACIONES
            // -----------------------

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Debe ingresar un correo válido.";
                return Page();
            }

            try { _ = new MailAddress(Email); }
            catch
            {
                ErrorMessage = "El correo ingresado no tiene un formato válido.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Asunto))
            {
                ErrorMessage = "El asunto es obligatorio.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Cuerpo))
            {
                ErrorMessage = "El cuerpo del mensaje es obligatorio.";
                return Page();
            }

            // Sanitizar HTML
            var safeBody = Regex.Replace(Cuerpo, "<script.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            safeBody = Regex.Replace(safeBody, "on[a-zA-Z]+=", "data-event=", RegexOptions.IgnoreCase);
            CuerpoSanitizado = safeBody;

            // Paso 1 — mostrar confirmación
            if (!Confirmado)
            {
                MostrarConfirmacion = true;
                return Page();
            }

            // Paso 2 — enviar
            var (ok, error) = await _notiApi.EnviarAsync(Email, Asunto, safeBody, userId.Value, token);

            if (!ok)
            {
                ErrorMessage = error ?? "No se pudo enviar la notificación.";
                MostrarConfirmacion = true;
                return Page();
            }

            EnvioExitoso = true;
            ResultadoEnvio = "Notificación enviada correctamente.";

            await _bitacoraApi.RegistrarAsync(userId.Value, $"Envió notificación a {Email}.");

            Historial = await _notiApi.GetAllAsync(token);

            return Page();
        }
    }
}
