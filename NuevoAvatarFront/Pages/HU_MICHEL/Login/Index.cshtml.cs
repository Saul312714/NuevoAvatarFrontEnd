using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.HU_MICHEL.Login
{
    public class IndexModel : PageModel
    {
        private readonly IAuthApiClient _authApi;
        private readonly IBitacoraApiClient _bitacoraApi;

        public IndexModel(IAuthApiClient authApi, IBitacoraApiClient bitacoraApi)
        {
            _authApi = authApi;
            _bitacoraApi = bitacoraApi;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public bool RememberMe { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // limpiar sesión previa
            HttpContext.Session.Clear();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validación del correo
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

            // Llamar al API de autenticación
            var login = await _authApi.LoginAsync(Email, Password);

            if (login == null)
            {
                await _bitacoraApi.RegistrarAsync(0, $"Login fallido ({Email})");
                ErrorMessage = "Usuario y/o contraseña incorrectos.";
                return Page();
            }

            // Guardar tokens en sesión
            HttpContext.Session.SetString("AccessToken", login.Access_Token);
            HttpContext.Session.SetString("RefreshToken", login.Refresh_Token);
            HttpContext.Session.SetInt32("UsuarioID", login.UsuarioID);

            // Recordarme: guarda también en cookies
            if (RememberMe)
            {
                var options = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    HttpOnly = false, 
                    Secure = false,  
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                };

                Response.Cookies.Append("AccessToken", login.Access_Token, options);
                Response.Cookies.Append("RefreshToken", login.Refresh_Token, options);
                Response.Cookies.Append("UsuarioID", login.UsuarioID.ToString(), options);
            }



            await _bitacoraApi.RegistrarAsync(login.UsuarioID, "Inicio de sesión exitoso");

            return Content("Login exitoso - prueba de cookies OK");
        }
    }
}
