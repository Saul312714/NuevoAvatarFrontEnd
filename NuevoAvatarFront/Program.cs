using Microsoft.AspNetCore.CookiePolicy;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Services;

var builder = WebApplication.CreateBuilder(args);


// CONFIGURACIÓN DE SERVICIOS
// Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// Sesión (IMPORTANTE: AddDistributedMemoryCache debe ir ANTES de AddSession)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// OBTENER URLs BASE DESDE CONFIGURACIÓN
string usuariosRolesBase = builder.Configuration["UsuariosRolesApi:BaseUrl"]
    ?? throw new Exception("❌ UsuariosRolesApi:BaseUrl no configurado");

string bitacoraBase = builder.Configuration["BitacoraApi:BaseUrl"]
    ?? throw new Exception("❌ BitacoraApi:BaseUrl no configurado");

string ofertaAcademicaBase = builder.Configuration["OfertaAcademicaApi:BaseUrl"]
    ?? throw new Exception("❌ OfertaAcademicaApi:BaseUrl no configurado");


// CLIENTES HTTP - USUARIOS & ROLES
builder.Services.AddHttpClient<IUsuarioApiClient, UsuarioApiClient>(client =>
    client.BaseAddress = new Uri(usuariosRolesBase));

builder.Services.AddHttpClient<IRolApiClient, RolApiClient>(client =>
    client.BaseAddress = new Uri(usuariosRolesBase));

builder.Services.AddHttpClient<IModuloApiClient, ModuloApiClient>(client =>
    client.BaseAddress = new Uri(usuariosRolesBase));

builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
    client.BaseAddress = new Uri(usuariosRolesBase));


// CLIENTES HTTP - BITÁCORA
builder.Services.AddHttpClient<IBitacoraApiClient, BitacoraApiClient>(client =>
    client.BaseAddress = new Uri(bitacoraBase));

// CLIENTES HTTP - OFERTA ACADÉMICA
builder.Services.AddHttpClient<IInstitucionApiClient, InstitucionApiClient>(client =>
    client.BaseAddress = new Uri(ofertaAcademicaBase));

builder.Services.AddHttpClient<IProfesorApiClient, ProfesorApiClient>(client =>
    client.BaseAddress = new Uri(ofertaAcademicaBase));

builder.Services.AddHttpClient<ICarreraApiClient, CarreraApiClient>(client =>
    client.BaseAddress = new Uri(ofertaAcademicaBase));

builder.Services.AddHttpClient<ICursoApiClient, CursoApiClient>(client =>
    client.BaseAddress = new Uri(ofertaAcademicaBase));


// HELPERS
builder.Services.AddScoped<TokenHelper>();

var app = builder.Build();

// MIDDLEWARE PIPELINE (ORDEN IMPORTANTE)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Sesión debe ir ANTES de Authentication y Authorization
app.UseSession();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    HttpOnly = HttpOnlyPolicy.None,
    Secure = CookieSecurePolicy.None
});

app.UseAuthentication();
app.UseAuthorization();


// RUTAS
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