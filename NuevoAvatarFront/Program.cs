using Microsoft.AspNetCore.CookiePolicy;
using NuevoAvatarFront.Helpers;
using System.Net.Http.Headers;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Services;

var builder = WebApplication.CreateBuilder(args);

// --------------------- Razor Pages ---------------------
builder.Services.AddRazorPages();

// --------------------- Sesiones ---------------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// --------------------- Token Helper ---------------------
builder.Services.AddScoped<TokenHelper>();


// =========================================================
//                CLIENTES PROPIOS 
// =========================================================

// API OFERTA
builder.Services.AddHttpClient("OfertaClient", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Apis:Oferta"] ?? "http://localhost:5048";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// API MATRÍCULA
builder.Services.AddHttpClient("MatriculaClient", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Apis:Matricula"] ?? "http://localhost:5193";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// API PROFESORES
builder.Services.AddHttpClient<ProfesorClient>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Apis:Oferta"] ?? "http://localhost:5048";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// API PERIODOS
builder.Services.AddHttpClient<PeriodoClient>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Apis:Oferta"] ?? "http://localhost:5048";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// Tus servicios
builder.Services.AddScoped<IPreMatriculaService, PreMatriculaService>();
builder.Services.AddScoped<IEstudianteService, EstudianteService>();
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<ICursoService, CursoService>();
builder.Services.AddScoped<IPeriodoService, PeriodoService>();
builder.Services.AddScoped<IMatriculaFrontService, MatriculaFrontService>();
builder.Services.AddScoped<IGrupoService, GrupoService>();

// =========================================================
//                CLIENTES DE DEVELOP (SEGURIDAD)
// =========================================================

// UsuariosRoles API
builder.Services.AddHttpClient<IUsuarioApiClient, UsuarioApiClient>(client =>
{
    var baseUrl = builder.Configuration["UsuariosRolesApi:BaseUrl"]
        ?? throw new InvalidOperationException("UsuariosRolesApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Roles API
builder.Services.AddHttpClient<IRolApiClient, RolApiClient>(client =>
{
    var baseUrl = builder.Configuration["UsuariosRolesApi:BaseUrl"]
        ?? throw new InvalidOperationException("UsuariosRolesApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Auth API
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
{
    var baseUrl = builder.Configuration["UsuariosRolesApi:BaseUrl"]
        ?? throw new InvalidOperationException("UsuariosRolesApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Bitácora API
builder.Services.AddHttpClient<IBitacoraApiClient, BitacoraApiClient>(client =>
{
    var baseUrl = builder.Configuration["BitacoraApi:BaseUrl"]
        ?? throw new InvalidOperationException("BitacoraApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});


var app = builder.Build();

// ================== MIDDLEWARE ==================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

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
app.UseAuthorization();

// Logout
app.MapPost("/logout", async (HttpContext context, IBitacoraApiClient bitacoraApi) =>
{
    var idUsuario = context.Session.GetInt32("UsuarioID") ?? 0;

    try
    {
        await bitacoraApi.RegistrarAsync(idUsuario, "Logout de sesión");
    }
    catch
    {
        Console.WriteLine("No se pudo registrar el logout en bitácora.");
    }

    context.Session.Clear();
    foreach (var cookie in context.Request.Cookies.Keys)
        context.Response.Cookies.Delete(cookie);

    context.Response.Redirect("/HU_MICHEL/Login/Index?logout=1");
});

// Redirect root
app.MapGet("/", context =>
{
    context.Response.Redirect("/HU_MICHEL/Login/Index");
    return Task.CompletedTask;
});

app.MapRazorPages();

app.Run();

