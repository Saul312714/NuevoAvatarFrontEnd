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

        public async Task<IActionResult> OnPostAsync()
        {
            // Validaciones básicas
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
            
            var login = await _authApi.LoginAsync(Email, Password);
            if (login == null)
            {
                await _bitacoraApi.RegistrarAsync(0, $"Login fallido ({Email})");
                ErrorMessage = "Usuario y/o contraseña incorrectos.";
                return Page();
            }

            // Guardar tokens y usuario ID
            HttpContext.Session.SetString("AccessToken", login.Access_Token);
            HttpContext.Session.SetString("RefreshToken", login.Refresh_Token);
            HttpContext.Session.SetInt32("UsuarioID", login.UsuarioID);

            //Obtener información del usuario desde la API UsuariosRoles
            var usuario = await _usuarioApi.GetByIdAsync(login.UsuarioID, login.UsuarioID);
            if (usuario != null)
            {
                HttpContext.Session.SetString("UserName", usuario.Nombre);
                HttpContext.Session.SetString("UserRole", usuario.RolNombre);
            }

            //Si “Recordarme” está activo, guardar cookies
            if (RememberMe)
            {
                var options = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7) };
                Response.Cookies.Append("AccessToken", login.Access_Token, options);
                Response.Cookies.Append("RefreshToken", login.Refresh_Token, options);
                Response.Cookies.Append("UsuarioID", login.UsuarioID.ToString(), options);
            }

            await _bitacoraApi.RegistrarAsync(login.UsuarioID, "Inicio de sesión exitoso");

            return RedirectToPage("/Ejemplo/Tables");
        }
    }
}
