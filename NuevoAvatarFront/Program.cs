using System.Net.Http.Headers;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Services;

var builder = WebApplication.CreateBuilder(args);

// --------------------- Servicios de Razor Pages ---------------------
builder.Services.AddRazorPages();

// --------------------- Servicios de Sesión ---------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --------------------- Middleware para agregar el token ---------------------
builder.Services.AddTransient<ApiTokenHandler>();

// --------------------- Servicios HTTP para las APIs ---------------------
// API de Oferta
builder.Services.AddHttpClient("OfertaClient", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Apis:Oferta"] ?? "http://localhost:5048";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}).AddHttpMessageHandler<ApiTokenHandler>();

// API de Matricula
builder.Services.AddHttpClient("MatriculaClient", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Apis:Matricula"] ?? "http://localhost:5193";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}).AddHttpMessageHandler<ApiTokenHandler>();

// API de Profesores (Oferta)
builder.Services.AddHttpClient<NuevoAvatarFront.Services.ProfesorClient>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Apis:Oferta"] ?? "http://localhost:5048";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}).AddHttpMessageHandler<ApiTokenHandler>();

// API de Periodos (Oferta)
builder.Services.AddHttpClient<NuevoAvatarFront.Services.PeriodoClient>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Apis:Oferta"] ?? "http://localhost:5048";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}).AddHttpMessageHandler<ApiTokenHandler>();

builder.Services.AddScoped<IPreMatriculaService, PreMatriculaService>();
builder.Services.AddScoped<IEstudianteService, EstudianteService>();
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<ICursoService, CursoService>();
builder.Services.AddScoped<IPeriodoService, PeriodoService>();
builder.Services.AddScoped<NuevoAvatarFront.Services.IMatriculaFrontService, NuevoAvatarFront.Services.MatriculaFrontService>();
builder.Services.AddScoped<IGrupoService, GrupoService>();

var app = builder.Build();

// --------------------- Configuración Middleware ---------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

