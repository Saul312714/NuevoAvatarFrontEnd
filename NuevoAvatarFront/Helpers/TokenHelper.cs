using Microsoft.AspNetCore.Http;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Helpers
{
    public class TokenHelper
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly IAuthApiClient _authApi;

        public TokenHelper(IHttpContextAccessor httpContext, IAuthApiClient authApi)
        {
            _httpContext = httpContext;
            _authApi = authApi;
        }

        public async Task<bool> EnsureTokenValidAsync()
        {
            var session = _httpContext.HttpContext!.Session;
            var accessToken = session.GetString("AccessToken");
            var refreshToken = session.GetString("RefreshToken");
            var expiresIn = session.GetString("ExpiresIn");

            if (accessToken == null || refreshToken == null || expiresIn == null)
                return false;

            var expiration = DateTime.Parse(expiresIn);
            if (DateTime.UtcNow < expiration)
                return true; // aún válido

            // Si expiró, renovamos
            var newTokens = await _authApi.RefreshAsync(refreshToken);
            if (newTokens == null)
                return false;

            // Guardar nuevos tokens
            session.SetString("AccessToken", newTokens.Access_Token);
            session.SetString("RefreshToken", newTokens.Refresh_Token);
            session.SetString("ExpiresIn", newTokens.Expires_In);
            return true;
        }
    }
}
