using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Xml.Linq;
using System;
using ClinicaServices;
using clinicaWeb.Security;

namespace clinicaWeb.Extensions;

public static class WebIocExtensions
{
    public static void WebInjections(this IServiceCollection services, IConfiguration configuration)
    {
        #region Application Services
        //En esta region se definen los servicios del backend a utilizar en los controladores del frontend
        //Ejemplo:
        //services.AddSingleton<IExampleServices, ExampleServices>();         
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<IUserServices, UserServices>();
        services.AddScoped<IConsultaServices, ConsultaServices>();
        services.AddScoped<IPacienteServices, PacienteServices>();
        services.AddScoped<IServiciosServices, ServiciosServices>();
        services.AddScoped<IDetallesServices, DetalleServices>();
        services.AddScoped<IRecetaServices, RecetaServices>();
        services.AddScoped<ICitaServices, CitaServices>();
        services.AddScoped<IErrorLogService, ErrorLogService>();
        services.AddScoped<IEstudioImagenService, EstudioImagenService>();
        services.AddScoped<IOrdenEstudioService, OrdenEstudioService>();
        services.AddScoped<ICatalogoIndicacionService, CatalogoIndicacionService>();
        // Storage: Azure Blob si hay StorageConnectionString, si no Local (wwwroot/uploads).
        var blobConn = configuration["StorageConnectionString"];
        var blobContainer = configuration["StorageContainerName"];
        if (string.IsNullOrWhiteSpace(blobContainer)) blobContainer = "mhsystem-prd";
        if (string.IsNullOrWhiteSpace(blobConn))
        {
            services.AddScoped<ClinicaServices.Storage.IStorageService, ClinicaServices.Storage.LocalStorageService>();
        }
        else
        {
            services.AddScoped<ClinicaServices.Storage.IStorageService>(sp =>
                new ClinicaServices.Storage.AzureBlobStorageService(blobConn, blobContainer));
        }
        #endregion





    }
}
