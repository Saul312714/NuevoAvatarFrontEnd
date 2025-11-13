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
                    var result = await _authApi.RefreshAsync(refreshToken!);

                    if (result == null)
                    {
                        context.Session.Clear();
                        context.Response.Redirect("/HU_MICHEL/Login/Index");
                        return null;
                    }

                    context.Session.SetString("AccessToken", result.Access_Token);
                    context.Session.SetString("RefreshToken", result.Refresh_Token);
                    return result.Access_Token;
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
