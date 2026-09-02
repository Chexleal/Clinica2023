using ClinicaServices;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;

public abstract class ErrorHandlingController : Controller
{
    protected void RegistrarError(Exception exception)
    {
        var errorLogService = HttpContext.RequestServices.GetRequiredService<IErrorLogService>();
        errorLogService.Registrar(
            exception,
            "Controlado",
            Request.Path,
            Request.Method,
            HttpContext.TraceIdentifier);
    }
}
