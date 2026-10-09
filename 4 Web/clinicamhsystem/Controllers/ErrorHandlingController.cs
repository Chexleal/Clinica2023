using ClinicaServices;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;

public abstract class ErrorHandlingController : Controller
{
    protected void RegistrarError(Exception exception)
    {
        var errorLogService = HttpContext.RequestServices.GetRequiredService<IErrorLogService>();
        var requestId = HttpContext.TraceIdentifier;
        var errorLogId = errorLogService.Registrar(
            exception,
            "Controlado",
            Request.Path,
            Request.Method,
            requestId);

        HttpContext.Items["ErrorLogId"] = errorLogId?.ToString();
        HttpContext.Items["ErrorRequestId"] = requestId;
    }
}
