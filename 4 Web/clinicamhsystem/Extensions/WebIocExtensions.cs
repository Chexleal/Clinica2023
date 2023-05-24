using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Xml.Linq;
using System;
using ClinicaServices;

namespace clinicaWeb.Extensions;

public static class WebIocExtensions
{
    public static void WebInjections(this IServiceCollection services)
    {
        services.AddMemoryCache().BuildServiceProvider();
        #region Application Services
        //En esta region se definen los servicios del backend a utilizar en los controladores del frontend
        //Ejemplo:
        //services.AddSingleton<IExampleServices, ExampleServices>();         
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IUserServices, UserServices>();
        services.AddScoped<IConsultaServices, ConsultaServices>();
        services.AddScoped<IPacienteServices, PacienteServices>();
        services.AddScoped<IServiciosServices, ServiciosServices>();
        services.AddScoped<IDetallesServices, DetalleServices>();
        services.AddScoped<IRecetaServices, RecetaServices>();
        services.AddScoped<ICitaServices, CitaServices>();
        #endregion





    }
}