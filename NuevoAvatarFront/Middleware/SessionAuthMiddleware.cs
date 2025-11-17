using Microsoft.AspNetCore.Http;

namespace NuevoAvatarFront.Middleware
{
    public class SessionAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Rutas permitidas sin autenticación
            var rutasPublicas = new[]
            {
                "/hu_michel/login",
                "/hu_michel/login/index",
                "/hu_michel/login/index?expired=1",
                "/hu_michel/login/index?logout=1",
                "/hu_michel/login/index?handler=logout",
                "/css/",
                "/js/",
                "/lib/",
                "/images/",
                "/vendor/",
                "/favicon.ico"
            };

            // Si la ruta es pública → dejar pasar
            if (rutasPublicas.Any(r => path.StartsWith(r)))
            {
                await _next(context);
                return;
            }

            // Validar que exista sesión
            var userId = context.Session.GetInt32("UsuarioID");
            var token = context.Session.GetString("AccessToken");

            if (userId == null || token == null)
            {
                context.Response.Redirect("/HU_MICHEL/Login/Index?expired=1");
                return;
            }

            await _next(context);
        }
    }

    // Extensión
    public static class SessionAuthMiddlewareExtension
    {
        public static IApplicationBuilder UseSessionAuth(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SessionAuthMiddleware>();
        }
    }
}
