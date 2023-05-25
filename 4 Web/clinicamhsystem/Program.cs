using ClinicaServices;
using clinicaWeb.Extensions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();



// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ClinicaContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
 
}, ServiceLifetime.Singleton);


builder.Host.ConfigureServices(services =>
{
    services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
    services.AddSession();
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

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
