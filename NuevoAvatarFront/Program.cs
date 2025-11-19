using Microsoft.AspNetCore.CookiePolicy;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Services;

var builder = WebApplication.CreateBuilder(args);


// ===============================
// RAZOR PAGES
// ===============================
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// Cliente HTTP para UsuariosRoles
builder.Services.AddHttpClient<IUsuarioApiClient, UsuarioApiClient>(client =>
// ===============================
// SESIONES
// ===============================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Registrar el cliente de Roles
builder.Services.AddHttpClient<IRolApiClient, RolApiClient>(client =>
{
    var baseUrl = builder.Configuration["UsuariosRolesApi:BaseUrl"]
        ?? throw new InvalidOperationException("UsuariosRolesApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});
// ===============================
// AUTO-MAPPER
// ===============================
builder.Services.AddAutoMapper(typeof(InstitucionProfile).Assembly);

// ===============================
// API CLIENTS
// ===============================
string usuariosRolesBase = builder.Configuration["UsuariosRolesApi:BaseUrl"]
    ?? throw new Exception("❌ UsuariosRolesApi:BaseUrl no configurado");

string bitacoraBase = builder.Configuration["BitacoraApi:BaseUrl"]
    ?? throw new Exception("❌ BitacoraApi:BaseUrl no configurado");

string ofertaAcademicaBase = builder.Configuration["OfertaAcademicaApi:BaseUrl"]
    ?? throw new Exception("❌ OfertaAcademicaApi:BaseUrl no configurado");

// Registrar otros clientes HTTP para autenticación y bitácora
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
{
    var baseUrl = builder.Configuration["UsuariosRolesApi:BaseUrl"]
        ?? throw new InvalidOperationException("UsuariosRolesApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});
// --- Usuarios & Roles ---
builder.Services.AddHttpClient<IUsuarioApiClient, UsuarioApiClient>(c =>
    c.BaseAddress = new Uri(usuariosRolesBase));

builder.Services.AddHttpClient<IModuloApiClient, ModuloApiClient>(c =>
    c.BaseAddress = new Uri(usuariosRolesBase));

builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(c =>
    c.BaseAddress = new Uri(usuariosRolesBase));

// --- Bitácora ---
builder.Services.AddHttpClient<IBitacoraApiClient, BitacoraApiClient>(c =>
    c.BaseAddress = new Uri(bitacoraBase));

// --- Oferta Académica ---
builder.Services.AddHttpClient<IInstitucionApiClient, InstitucionApiClient>(c =>
    c.BaseAddress = new Uri(ofertaAcademicaBase));

builder.Services.AddHttpClient<IBitacoraApiClient, BitacoraApiClient>(client =>
{
    var baseUrl = builder.Configuration["BitacoraApi:BaseUrl"]
        ?? throw new InvalidOperationException("BitacoraApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddHttpClient<IProfesorApiClient, ProfesorApiClient>(c =>
    c.BaseAddress = new Uri(ofertaAcademicaBase));

builder.Services.AddHttpClient<ICarreraApiClient, CarreraApiClient>(c =>
    c.BaseAddress = new Uri(ofertaAcademicaBase));

// Agregar HttpContextAccessor y TokenHelper
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<ICursoApiClient, CursoApiClient>(c =>
    c.BaseAddress = new Uri(ofertaAcademicaBase));

// ===============================
// HELPERS
// ===============================
builder.Services.AddScoped<TokenHelper>();

var app = builder.Build();

// ===============================
// MIDDLEWARE PIPELINE
// ===============================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    HttpOnly = HttpOnlyPolicy.None,
    Secure = CookieSecurePolicy.None
});

app.UseSession();

// AUTENTICACIÓN → OPCIONAL PERO RECOMENDADO
app.UseAuthentication();

app.UseAuthorization();

// ===============================
// RUTAS
// ===============================
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

app.MapGet("/", context =>
{
    context.Response.Redirect("/HU_MICHEL/Login/Index");
    return Task.CompletedTask;
});

app.MapRazorPages();

app.Run();
