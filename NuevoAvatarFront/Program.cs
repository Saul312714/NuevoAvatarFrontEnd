using Microsoft.AspNetCore.CookiePolicy;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

/************************************/

// Cliente HTTP para UsuariosRoles
builder.Services.AddHttpClient<IUsuarioApiClient, UsuarioApiClient>(client =>
{
    var baseUrl = builder.Configuration["UsuariosRolesApi:BaseUrl"]
        ?? throw new InvalidOperationException("UsuariosRolesApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpContextAccessor(); 



builder.Services.AddHttpClient<IModuloApiClient, ModuloApiClient>(client =>
{
    var baseUrl = builder.Configuration["UsuariosRolesApi:BaseUrl"]
        ?? throw new InvalidOperationException("UsuariosRolesApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});



// Cliente HTTP para autenticación
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
{
    var baseUrl = builder.Configuration["UsuariosRolesApi:BaseUrl"]
        ?? throw new InvalidOperationException("UsuariosRolesApi:BaseUrl no configurad");
    client.BaseAddress = new Uri(baseUrl);
});

// Cliente HTTP para bitácora
builder.Services.AddHttpClient<IBitacoraApiClient, BitacoraApiClient>(client =>
{
    var baseUrl = builder.Configuration["BitacoraApi:BaseUrl"]
        ?? throw new InvalidOperationException("BitacoraApi:BaseUrl no configurado");
    client.BaseAddress = new Uri(baseUrl);
});

// Habilitar sesiones para guardar tokens
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenHelper>();

/*************************************/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

// Política de cookies para permitir personalizadas en localhost
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,  // permite uso básico
    HttpOnly = HttpOnlyPolicy.None,            // deja verlas en DevTools
    Secure = CookieSecurePolicy.None           // permite cookies en HTTP
});


app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapPost("/logout", async (HttpContext context, IBitacoraApiClient bitacoraApi) =>
{
    var idUsuario = context.Session.GetInt32("UsuarioID") ?? 0;

    try
    {
        await bitacoraApi.RegistrarAsync(idUsuario, "Logout de sesión");
    }
    catch
    {
        Console.WriteLine("⚠️ No se pudo registrar el logout en bitácora.");
    }

    //Limpiar sesión y cookies
    context.Session.Clear();
    foreach (var cookie in context.Request.Cookies.Keys)
        context.Response.Cookies.Delete(cookie);

    //Guardar bandera temporal (querystring)
    context.Response.Redirect("/HU_MICHEL/Login/Index?logout=1");
});



app.MapGet("/", context =>
{
    context.Response.Redirect("/HU_MICHEL/Login/Index");
    return Task.CompletedTask;
});

app.MapRazorPages();

app.Run();
