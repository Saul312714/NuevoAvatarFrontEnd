using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace NuevoAvatarFront.Infra
{
    public class ApiTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly IConfiguration _config;

        public ApiTokenHandler(IHttpContextAccessor httpContext, IConfiguration config)
        {
            _httpContext = httpContext;
            _config = config;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            
            var token = _httpContext.HttpContext?.Session.GetString("JWT");

            
            if (string.IsNullOrWhiteSpace(token))
                token = _config["Auth:DevBearerToken"];

            
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                Console.WriteLine("No se encontró token para adjuntar en la solicitud HTTP.");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}