namespace NuevoAvatarFront.Infra
{
    using Microsoft.AspNetCore.Http;
    using System.Net.Http.Headers;

    public abstract class ApiClientBase
    {
        private readonly IHttpContextAccessor _ctx;
        protected ApiClientBase(IHttpContextAccessor ctx) => _ctx = ctx;

        protected void AttachBearer(HttpClient http)
        {
            var token = _ctx.HttpContext?.Session.GetString("access_token");
            if (!string.IsNullOrWhiteSpace(token))
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
