using System.Net.Http.Headers;
using Microsoft.AspNetCore.CookiePolicy;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Middleware;
using NuevoAvatarFront.Services;
using NuevoAvatarFront.Services.Dashboard;
using NuevoAvatarFront.Pages.Expedientes; 

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// =========================================================
// RAZOR PAGES + ACCESOR
// =========================================================
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

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

// =========================================================
// HELPERS
// =========================================================
builder.Services.AddScoped<TokenHelper>();

// =========================================================
// NAMED HTTP CLIENTS BÁSICOS
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

// ✅ API PAGOS (named client) - PARA ADM20 / Facturas
builder.Services.AddHttpClient("PagosClient", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    // Usa la sección que ya tienes en appsettings.json
    var baseUrl = cfg["PagosApi:BaseUrl"] ?? "http://localhost:5034";
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

// =========================================================
// BASE URLS (APIs tipo "XxxApi:BaseUrl")
// =========================================================
var baseUsuariosRoles = builder.Configuration["UsuariosRolesApi:BaseUrl"]
    ?? throw new InvalidOperationException("UsuariosRolesApi:BaseUrl no configurado");

var baseBitacora = builder.Configuration["BitacoraApi:BaseUrl"]
    ?? throw new InvalidOperationException("BitacoraApi:BaseUrl no configurado");

var baseOferta = builder.Configuration["OfertaAcademicaApi:BaseUrl"]
    ?? throw new InvalidOperationException("OfertaAcademicaApi:BaseUrl no configurado");

var baseMatricula = builder.Configuration["MatriculaApi:BaseUrl"]
    ?? throw new InvalidOperationException("MatriculaApi:BaseUrl no configurado");

// =========================================================
// CLIENTES: SEGURIDAD (Usuarios / Roles / Auth / Parámetros / Módulos)
// =========================================================
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

builder.Services.AddHttpClient<IModuloApiClient, ModuloApiClient>(c =>
{
    c.BaseAddress = new Uri(baseUsuariosRoles);
});

builder.Services.AddHttpClient<IRolModuloApiClient, RolModuloApiClient>(c =>
{
    c.BaseAddress = new Uri(baseUsuariosRoles);
});

builder.Services.AddHttpClient<IParametroApiClient, ParametroApiClient>(c =>
{
    c.BaseAddress = new Uri(baseUsuariosRoles);
});

// =========================================================
// CLIENTES: BITÁCORA
// =========================================================
builder.Services.AddHttpClient<IBitacoraApiClient, BitacoraApiClient>(c =>
{
    c.BaseAddress = new Uri(baseBitacora);
});

// =========================================================
// CLIENTES: OFERTA ACADÉMICA (Institución / Carrera / Curso / Profesor / Grupos)
// =========================================================
builder.Services.AddHttpClient<IInstitucionApiClient, InstitucionApiClient>(c =>
{
    c.BaseAddress = new Uri(baseOferta);
});

builder.Services.AddHttpClient<ICarreraApiClient, CarreraApiClient>(c =>
{
    c.BaseAddress = new Uri(baseOferta);
});

builder.Services.AddHttpClient<ICursoApiClient, CursoApiClient>(c =>
{
    c.BaseAddress = new Uri(baseOferta);
});

builder.Services.AddHttpClient<IProfesorApiClient, ProfesorApiClient>(c =>
{
    c.BaseAddress = new Uri(baseOferta);
});

builder.Services.AddHttpClient<IGrupoApiClient, GrupoApiClient>(c =>
{
    c.BaseAddress = new Uri(baseOferta);
});

// =========================================================
// CLIENTES: MATRÍCULA / PREMATRÍCULA / EXPEDIENTES
// =========================================================
builder.Services.AddHttpClient<IExpedienteApiClient, ExpedienteApiClient>(c =>
{
    c.BaseAddress = new Uri(baseMatricula);
});

// Servicios internos 
builder.Services.AddScoped<IPreMatriculaService, PreMatriculaService>();
builder.Services.AddScoped<IEstudianteService, EstudianteService>();
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<ICursoService, CursoService>();
builder.Services.AddScoped<IPeriodoService, PeriodoService>();
builder.Services.AddScoped<IMatriculaFrontService, MatriculaFrontService>();
builder.Services.AddScoped<IGrupoService, GrupoService>();
builder.Services.AddScoped<IFacturaFrontService, FacturaFrontService>();

// =========================================================
// DASHBOARD
// =========================================================
builder.Services.AddHttpClient<IDashboardService, DashboardService>();

// =========================================================
// Mocks (desarrollo)
// =========================================================
builder.Services.AddSingleton<IMockDirecciones, MockDirecciones>();

// =========================================================
// BUILD APP
// =========================================================
var app = builder.Build();

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

// =========================================================
// LOGOUT
// =========================================================
app.MapPost("/logout", async (HttpContext context, IBitacoraApiClient bitacoraApi) =>
{
    var idUsuario = context.Session.GetInt32("UsuarioID") ?? 0;

    try
    {
        await bitacoraApi.RegistrarAsync(idUsuario, "Logout de sesión");
    }
    catch
    {
        Console.WriteLine("⚠ No se pudo registrar el logout en bitácora.");
    }

    context.Session.Clear();
    foreach (var cookie in context.Request.Cookies.Keys)
        context.Response.Cookies.Delete(cookie);

    context.Response.Redirect("/HU_MICHEL/Login/Index?logout=1");
});

// =========================================================
// HOME REDIRECT
// =========================================================
app.MapGet("/", ctx =>
{
    ctx.Response.Redirect("/HU_MICHEL/Login/Index");
    return Task.CompletedTask;
});

app.MapRazorPages();
app.Run();
