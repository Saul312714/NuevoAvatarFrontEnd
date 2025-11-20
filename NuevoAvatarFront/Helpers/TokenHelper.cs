using Microsoft.AspNetCore.Http;
using NuevoAvatarFront.Services;
using System.IdentityModel.Tokens.Jwt;

namespace NuevoAvatarFront.Helpers
{
    public class TokenHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthApiClient _authApi;

        public TokenHelper(IHttpContextAccessor httpContextAccessor, IAuthApiClient authApi)
        {
            _httpContextAccessor = httpContextAccessor;
            _authApi = authApi;
        }

        public async Task<string?> EnsureValidAccessTokenAsync()
        {
            var context = _httpContextAccessor.HttpContext!;
            var token = context.Session.GetString("AccessToken");
            var refreshToken = context.Session.GetString("RefreshToken");

            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var exp = jwt.ValidTo;

            //Si el token está por expirar (menos de 2 minutos)
            if (exp <= DateTime.UtcNow.AddMinutes(2))
            {
                try
                {
                    var (data, error) = await _authApi.RefreshAsync(refreshToken!);

                    // Si hubo error (401, 400, 500, etc.)
                    if (error != null || data == null)
                    {
                        context.Session.Clear();
                        context.Response.Redirect("/HU_MICHEL/Login/Index");
                        return null;
                    }

                    // Guardar tokens renovados
                    context.Session.SetString("AccessToken", data.Access_Token);
                    context.Session.SetString("RefreshToken", data.Refresh_Token);

                    return data.Access_Token;
                }
                catch
                {
                    context.Session.Clear();
                    context.Response.Redirect("/HU_MICHEL/Login/Index");
                    return null;
                }
            }

            return token;
        }
    }
}
