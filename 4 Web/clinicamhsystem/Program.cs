using ClinicaServices;
using clinicaWeb.Extensions;
//using DinkToPdf;
//using DinkToPdf.Contracts;
using Microsoft.EntityFrameworkCore;
//using Microsoft.Identity.Client;
//using WkHtmlToPdfDotNet;
//using WkHtmlToPdfDotNet.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();


//Pdf Services
//var context = new CustomAsemblyLoadContext();
//context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), @"LibreriaPDF32\libwkhtmltox.dll"));
//builder.Services.AddSingleton(typeof(IConverter), new BasicConverter(new PdfTools()));


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ClinicaContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
 
}, ServiceLifetime.Scoped);


builder.Host.ConfigureServices(services =>
{
    services.AddRazorPages();
    services.AddRazorPages().AddRazorRuntimeCompilation();
});
builder.Services.WebInjections();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
