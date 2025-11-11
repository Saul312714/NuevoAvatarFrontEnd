using Microsoft.AspNetCore.CookiePolicy;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

/************************************/

// Cliente HTTP para autenticación
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
{
    var baseUrl = builder.Configuration["AuthApi:BaseUrl"]
        ?? throw new InvalidOperationException("AuthApi:BaseUrl no configurado");
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

app.MapGet("/", context =>
{
    context.Response.Redirect("/HU_MICHEL/Login/Index");
    return Task.CompletedTask;
});

app.MapRazorPages();

app.Run();
