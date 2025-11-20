using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.HU_MICHEL.Login
{
    public class IndexModel : PageModel
    {
        private readonly IAuthApiClient _authApi;
        private readonly IBitacoraApiClient _bitacoraApi;
        private readonly IUsuarioApiClient _usuarioApi;

        public IndexModel(IAuthApiClient authApi, IBitacoraApiClient bitacoraApi, IUsuarioApiClient usuarioApi)
        {
            _authApi = authApi;
            _bitacoraApi = bitacoraApi;
            _usuarioApi = usuarioApi;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public bool RememberMe { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet() => HttpContext.Session.Clear();

        public async Task<IActionResult> OnGetLogoutAsync()
        {
            var idUsuario = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            try
            {
                await _bitacoraApi.RegistrarAsync(idUsuario, "Logout de sesión");
            }
            catch { }

            HttpContext.Session.Clear();

            foreach (var cookie in Request.Cookies.Keys)
                Response.Cookies.Delete(cookie);

            return Redirect("/HU_MICHEL/Login/Index?logout=1");
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            {
                ErrorMessage = "El correo electrónico es obligatorio.";
                return Page();
            }

            if (!Email.EndsWith("@cuc.cr") && !Email.EndsWith("@cuc.ac.cr"))
            {
                ErrorMessage = "El correo debe pertenecer a los dominios cuc.cr o cuc.ac.cr.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "La contraseña es requerida.";
                return Page();
            }

            // LOGIN
            var (loginData, loginError) = await _authApi.LoginAsync(Email, Password);

            if (loginError != null || loginData == null)
            {
                ErrorMessage = loginError?.Message ?? "Credenciales inválidas.";
                return Page();
            }

            var login = loginData;

            // GUARDAR TOKENS
            HttpContext.Session.SetString("AccessToken", login.Access_Token);
            HttpContext.Session.SetString("RefreshToken", login.Refresh_Token);
            HttpContext.Session.SetInt32("UsuarioID", login.UsuarioID);

            // OBTENER INFO COMPLETA DEL USUARIO
            var (okUser, _, usuarioData, msgUser) =
                await _usuarioApi.GetByIdAsync(login.UsuarioID, login.UsuarioID);

            if (okUser && usuarioData != null)
            {
                HttpContext.Session.SetString("UserName", usuarioData.Nombre);
                HttpContext.Session.SetString("UserRole", usuarioData.RolNombre);
                HttpContext.Session.SetInt32("RolID", usuarioData.ID_Rol);
            }
            else
            {
                HttpContext.Session.SetString("UserName", "Usuario");
                HttpContext.Session.SetString("UserRole", "N/D");
                HttpContext.Session.SetInt32("RolID", 0);
            }


            // RECORDARME
            if (RememberMe)
            {
                var opt = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7) };
                Response.Cookies.Append("AccessToken", login.Access_Token, opt);
                Response.Cookies.Append("RefreshToken", login.Refresh_Token, opt);
                Response.Cookies.Append("UsuarioID", login.UsuarioID.ToString(), opt);
            }

            await _bitacoraApi.RegistrarAsync(login.UsuarioID, "Inicio de sesión exitoso");

            return RedirectToPage("/Dashboard/Index");
        }

    }
}
