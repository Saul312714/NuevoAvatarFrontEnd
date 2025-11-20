using Microsoft.AspNetCore.CookiePolicy;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Middleware;
using System.Net.Http.Headers;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Services;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// RAZOR PAGES
// =========================================================
builder.Services.AddRazorPages();

// =========================================================
// SESIÓN
// =========================================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenHelper>();

// =========================================================
// CLIENTES PROPIOS (OFERTA / MATRÍCULA / PREMATRÍCULA)
// =========================================================

// API OFERTA (named client)
builder.Services.AddHttpClient("OfertaClient", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Apis:Oferta"] ?? "http://localhost:5048";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// API MATRÍCULA (named client)
builder.Services.AddHttpClient("MatriculaClient", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Apis:Matricula"] ?? "http://localhost:5193";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// API PROFESORES (typed client)
builder.Services.AddHttpClient<ProfesorClient>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Apis:Oferta"] ?? "http://localhost:5048";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// API PERÍODOS (typed client)
builder.Services.AddHttpClient<PeriodoClient>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Apis:Oferta"] ?? "http://localhost:5048";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Servicios de tu módulo de prematrícula
builder.Services.AddScoped<IPreMatriculaService, PreMatriculaService>();
builder.Services.AddScoped<IEstudianteService, EstudianteService>();
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<ICursoService, CursoService>();
builder.Services.AddScoped<IPeriodoService, PeriodoService>();
builder.Services.AddScoped<IMatriculaFrontService, MatriculaFrontService>();
builder.Services.AddScoped<IGrupoService, GrupoService>();

// =========================================================
// CLIENTES DE SEGURIDAD (USUARIOS / ROLES / BITÁCORA)
// =========================================================
var baseUsuariosRoles = builder.Configuration["UsuariosRolesApi:BaseUrl"]
    ?? throw new InvalidOperationException("UsuariosRolesApi:BaseUrl no configurado");

var baseBitacora = builder.Configuration["BitacoraApi:BaseUrl"]
    ?? throw new InvalidOperationException("BitacoraApi:BaseUrl no configurado");

// Usuarios
builder.Services.AddHttpClient<IUsuarioApiClient, UsuarioApiClient>(c =>
{
    c.BaseAddress = new Uri(baseUsuariosRoles);
});

// Roles
builder.Services.AddHttpClient<IRolApiClient, RolApiClient>(c =>
{
    c.BaseAddress = new Uri(baseUsuariosRoles);
});

// Auth
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(c =>
{
    c.BaseAddress = new Uri(baseUsuariosRoles);
});

// Bitácora
builder.Services.AddHttpClient<IBitacoraApiClient, BitacoraApiClient>(c =>
{
    c.BaseAddress = new Uri(baseBitacora);
});

// Módulos
builder.Services.AddHttpClient<IModuloApiClient, ModuloApiClient>(c =>
{
    c.BaseAddress = new Uri(baseUsuariosRoles);
});

// Rol-Módulo
builder.Services.AddHttpClient<IRolModuloApiClient, RolModuloApiClient>(c =>
{
    c.BaseAddress = new Uri(baseUsuariosRoles);
});

// =========================================================
// BUILD APP
// =========================================================
var app = builder.Build();

// ================== MIDDLEWARE ==================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Política de cookies
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


