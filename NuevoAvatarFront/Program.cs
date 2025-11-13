var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorPages();


builder.Services.AddHttpClient();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // tiempo de expiración de sesión
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// builder.Services.AddAuthentication(...);

var app = builder.Build();

// ----------------------------------------------------------
// 🔹 Configurar el pipeline HTTP
// ----------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
