using ClinicaServices;
using clinicaWeb.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
//using DinkToPdf;
//using DinkToPdf.Contracts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Secretos locales (NO se commitea, ver .gitignore).
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index";
        options.AccessDeniedPath = "/Home/Index";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });



// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ClinicaContext>(options =>
{
    // La cadena viene de configuración (nunca hardcodeada):
    // - Azure App Service: Configuration -> Connection strings -> DefaultConnection (SQLAzure)
    // - Local: variable de entorno ConnectionStrings__DefaultConnection o user-secrets
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        connectionString =
            Environment.GetEnvironmentVariable("SQLAZURECONNSTR_DefaultConnection")
            ?? Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection")
            ?? Environment.GetEnvironmentVariable("CUSTOMCONNSTR_DefaultConnection");
    }

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Falta la cadena de conexión 'DefaultConnection'. Configúrala con la variable de entorno 'ConnectionStrings__DefaultConnection' o como cadena de conexión 'DefaultConnection' en el App Service.");
    }

    options.UseSqlServer(connectionString);
}, ServiceLifetime.Scoped);


builder.Host.ConfigureServices(services =>
{
    services.AddRazorPages();
    services.AddRazorPages().AddRazorRuntimeCompilation();
});
builder.Services.WebInjections(builder.Configuration);

var app = builder.Build();

// Fechas, monedas y validaciones en español (Guatemala): evita que el servidor en inglés
// muestre fechas como M/d/yyyy. Los formatos explícitos dd/MM/yyyy ya quedan fijos.
var culturaEsGt = new System.Globalization.CultureInfo("es-GT");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culturaEsGt;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culturaEsGt;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaEsGt),
    SupportedCultures = new List<System.Globalization.CultureInfo> { culturaEsGt },
    SupportedUICultures = new List<System.Globalization.CultureInfo> { culturaEsGt }
});

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Home/Error");

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
