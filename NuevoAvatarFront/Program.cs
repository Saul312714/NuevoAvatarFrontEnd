using Microsoft.AspNetCore.CookiePolicy;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Middleware;
using NuevoAvatarFront.Services;
using NuevoAvatarFront.Services.Dashboard;
using NuevoAvatarFront.Pages.Expedientes;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// RAZOR PAGES
// ======================================================
builder.Services.AddRazorPages();

// ======================================================
// BASE URLs (de appsettings.json)
// ======================================================
var baseUsuariosRoles = builder.Configuration["UsuariosRolesApi:BaseUrl"]
    ?? throw new InvalidOperationException("UsuariosRolesApi:BaseUrl no configurado");

var baseBitacora = builder.Configuration["BitacoraApi:BaseUrl"]
    ?? throw new InvalidOperationException("BitacoraApi:BaseUrl no configurado");

var baseOfertaAcademica = builder.Configuration["OfertaAcademicaApi:BaseUrl"]
    ?? throw new InvalidOperationException("OfertaAcademicaApi:BaseUrl no configurado");

var baseMatricula = builder.Configuration["MatriculaApi:BaseUrl"]
    ?? throw new InvalidOperationException("MatriculaApi:BaseUrl no configurado");

// ======================================================
// HTTP CLIENTS — CORRECTOS Y COMPLETOS
// ======================================================

// --- Usuarios / Roles / Login ---
builder.Services.AddHttpClient<IUsuarioApiClient, UsuarioApiClient>(c => c.BaseAddress = new Uri(baseUsuariosRoles));
builder.Services.AddHttpClient<IRolApiClient, RolApiClient>(c => c.BaseAddress = new Uri(baseUsuariosRoles));
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(c => c.BaseAddress = new Uri(baseUsuariosRoles));
builder.Services.AddHttpClient<IModuloApiClient, ModuloApiClient>(c => c.BaseAddress = new Uri(baseUsuariosRoles));
builder.Services.AddHttpClient<IRolModuloApiClient, RolModuloApiClient>(c => c.BaseAddress = new Uri(baseUsuariosRoles));

// --- Bitácora ---
builder.Services.AddHttpClient<IBitacoraApiClient, BitacoraApiClient>(c => c.BaseAddress = new Uri(baseBitacora));

// --- Parámetros ---
builder.Services.AddHttpClient<IParametroApiClient, ParametroApiClient>(c => c.BaseAddress = new Uri(baseUsuariosRoles));

// --- Grupos ---
builder.Services.AddHttpClient<IGrupoApiClient, GrupoApiClient>(c => c.BaseAddress = new Uri(baseOfertaAcademica));

// --- Expedientes ---
builder.Services.AddHttpClient<IExpedienteApiClient, ExpedienteApiClient>(c => c.BaseAddress = new Uri(baseMatricula));

// --- Dashboard ---
builder.Services.AddHttpClient<IDashboardService, DashboardService>(); // importante

// ======================================================
// MOCKS (solo Expedientes)
// ======================================================
builder.Services.AddSingleton<IMockDirecciones, MockDirecciones>();

// ======================================================
// SESIÓN
// ======================================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenHelper>();

// ======================================================
// BUILD
// ======================================================
var app = builder.Build();

// ======================================================
// PIPELINE
// ======================================================
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

// ======================================================
// LOGOUT
// ======================================================
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

// ======================================================
// REDIRECCIÓN DEFAULT
// ======================================================
app.MapGet("/", context =>
{
    context.Response.Redirect("/HU_MICHEL/Login/Index");
    return Task.CompletedTask;
});

app.MapRazorPages();
app.Run();
