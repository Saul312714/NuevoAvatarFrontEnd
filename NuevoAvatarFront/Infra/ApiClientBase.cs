using Microsoft.AspNetCore.Http;
using NuevoAvatarFront.Helpers;
using System.Net.Http.Headers;

namespace NuevoAvatarFront.Infra
{
    public abstract class ApiClientBase
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly TokenHelper _tokenHelper;

        protected ApiClientBase(IHttpContextAccessor httpContextAccessor, TokenHelper tokenHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
        }

        protected async Task AttachBearerAsync(HttpClient client)
        {
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("⚠ No se encontró un AccessToken válido en sesión.");
                return;
            }

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}

