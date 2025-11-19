using Microsoft.AspNetCore.CookiePolicy;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Middleware;
using NuevoAvatarFront.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// RAZOR PAGES
// ============================================
builder.Services.AddRazorPages();

// ============================================
// BASE URLS
// ============================================
var baseUsuariosRoles = builder.Configuration["UsuariosRolesApi:BaseUrl"]
    ?? throw new InvalidOperationException("UsuariosRolesApi:BaseUrl no configurado");

var baseBitacora = builder.Configuration["BitacoraApi:BaseUrl"]
    ?? throw new InvalidOperationException("BitacoraApi:BaseUrl no configurado");

// ============================================
// HTTP CLIENTS (solo los que EXISTEN)
// ============================================
builder.Services.AddHttpClient<IUsuarioApiClient, UsuarioApiClient>(c =>
{
    c.BaseAddress = new Uri(baseUsuariosRoles);
});

builder.Services.AddHttpClient<IRolApiClient, RolApiClient>(c =>
{
    c.BaseAddress = new Uri(baseUsuariosRoles);
});

builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(c =>
{
    c.BaseAddress = new Uri(baseUsuariosRoles);
});

builder.Services.AddHttpClient<IBitacoraApiClient, BitacoraApiClient>(c =>
{
    c.BaseAddress = new Uri(baseBitacora);
});

builder.Services.AddHttpClient<IModuloApiClient, ModuloApiClient>(c =>
{
    c.BaseAddress = new Uri(baseUsuariosRoles);
});

builder.Services.AddHttpClient<IRolModuloApiClient, RolModuloApiClient>(client =>
{
    client.BaseAddress = new Uri(baseUsuariosRoles);
});


builder.Services.AddHttpClient<IRolModuloApiClient, RolModuloApiClient>(client =>
{
    var baseUrl = builder.Configuration["UsuariosRolesApi:BaseUrl"]
        ?? throw new InvalidOperationException("UsuariosRolesApi:BaseUrl no configurado");

    client.BaseAddress = new Uri(baseUrl);
});

// ============================================
// SESIÓN
// ============================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenHelper>();

// ============================================
// APP
// ============================================
var app = builder.Build();

// ============================================
// PIPELINE
// ============================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    HttpOnly = HttpOnlyPolicy.None,
    Secure = CookieSecurePolicy.None
});

app.UseRouting();
app.UseSession();
app.UseSessionAuth();
app.UseAuthorization();

// ============================================
// LOGOUT
// ============================================
app.MapPost("/logout", async (HttpContext context, IBitacoraApiClient bitacoraApi) =>
{
    var idUsuario = context.Session.GetInt32("UsuarioID") ?? 0;

    try
    {
        await bitacoraApi.RegistrarAsync(idUsuario, "Logout de sesión");
    }
    catch
    {
        Console.WriteLine("No se pudo registrar la bitácora.");
    }

    context.Session.Clear();
    foreach (var cookie in context.Request.Cookies.Keys)
        context.Response.Cookies.Delete(cookie);

    context.Response.Redirect("/HU_MICHEL/Login/Index?logout=1");
});

// ============================================
// DEFAULT ROUTE → LOGIN
// ============================================
app.MapGet("/", context =>
{
    context.Response.Redirect("/HU_MICHEL/Login/Index");
    return Task.CompletedTask;
});

app.MapRazorPages();
app.Run();
