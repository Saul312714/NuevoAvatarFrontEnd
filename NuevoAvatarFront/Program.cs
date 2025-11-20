using Microsoft.AspNetCore.CookiePolicy;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Middleware;
using NuevoAvatarFront.Services;

var builder = WebApplication.CreateBuilder(args);

// RAZOR PAGES
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();


// SESIÓN (DEBE IR ANTES DE AddSession)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// BASE URLS
var baseUsuariosRoles = builder.Configuration["UsuariosRolesApi:BaseUrl"]
    ?? throw new InvalidOperationException("UsuariosRolesApi:BaseUrl no configurado");

var baseBitacora = builder.Configuration["BitacoraApi:BaseUrl"]
    ?? throw new InvalidOperationException("BitacoraApi:BaseUrl no configurado");

var baseOferta = builder.Configuration["OfertaAcademicaApi:BaseUrl"]
    ?? throw new InvalidOperationException("OfertaAcademicaApi:BaseUrl no configurado");

// HTTP CLIENTS – USUARIOS & ROLES
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

// HTTP CLIENTS – BITÁCORA
builder.Services.AddHttpClient<IBitacoraApiClient, BitacoraApiClient>(c =>
{
    c.BaseAddress = new Uri(baseBitacora);
});

// HTTP CLIENTS – OFERTA ACADÉMICA
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

// HELPERS
builder.Services.AddScoped<TokenHelper>();

// APP
var app = builder.Build();

// PIPELINE

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseSession();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    HttpOnly = HttpOnlyPolicy.None,
    Secure = CookieSecurePolicy.None
});

app.UseSessionAuth();

app.UseAuthorization();


// LOGOUT
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

    // Limpiar sesión y cookies
    context.Session.Clear();
    foreach (var cookie in context.Request.Cookies.Keys)
        context.Response.Cookies.Delete(cookie);

    context.Response.Redirect("/HU_MICHEL/Login/Index?logout=1");
});


// HOME → LOGIN
app.MapGet("/", context =>
{
    context.Response.Redirect("/HU_MICHEL/Login/Index");
    return Task.CompletedTask;
});

// RAZOR PAGES
app.MapRazorPages();

app.Run();
