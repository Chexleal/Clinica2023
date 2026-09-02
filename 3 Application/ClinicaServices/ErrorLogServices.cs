using ClinicaDomain;
using Microsoft.Extensions.Logging;

namespace ClinicaServices;

public interface IErrorLogService
{
    void Registrar(
        Exception exception,
        string tipoError,
        string? ruta = null,
        string? metodoHttp = null,
        string? traceIdentifier = null);

    Task RegistrarAsync(
        Exception exception,
        string tipoError,
        string? ruta = null,
        string? metodoHttp = null,
        string? traceIdentifier = null);
}

public sealed class ErrorLogService : IErrorLogService
{
    private readonly ClinicaContext _dbContext;
    private readonly ILogger<ErrorLogService> _logger;

    public ErrorLogService(
        ClinicaContext dbContext,
        ILogger<ErrorLogService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task RegistrarAsync(
        Exception exception,
        string tipoError,
        string? ruta = null,
        string? metodoHttp = null,
        string? traceIdentifier = null)
    {
        try
        {
            PrepararRegistro(exception, tipoError, ruta, metodoHttp, traceIdentifier);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception registroException)
        {
            // El registro de errores nunca debe ocultar el error original.
            _logger.LogError(registroException, "No fue posible guardar el error en ErrorLog.");
        }
    }

    public void Registrar(
        Exception exception,
        string tipoError,
        string? ruta = null,
        string? metodoHttp = null,
        string? traceIdentifier = null)
    {
        try
        {
            PrepararRegistro(exception, tipoError, ruta, metodoHttp, traceIdentifier);
            _dbContext.SaveChanges();
        }
        catch (Exception registroException)
        {
            _logger.LogError(registroException, "No fue posible guardar el error en ErrorLog.");
        }
    }

    private void PrepararRegistro(
        Exception exception,
        string tipoError,
        string? ruta,
        string? metodoHttp,
        string? traceIdentifier)
    {
        var error = new ErrorLog
        {
            IdErrorLog = Guid.NewGuid(),
            TipoError = Limitar(tipoError, 50) ?? string.Empty,
            Mensaje = Limitar(exception.Message, 500) ?? string.Empty,
            Detalle = Limitar(exception.ToString(), 4000),
            StackTrace = Limitar(exception.StackTrace, 8000),
            Ruta = Limitar(ruta, 500),
            MetodoHttp = Limitar(metodoHttp, 20),
            TraceIdentifier = Limitar(traceIdentifier, 100),
            Nivel = "Error",
            Resuelto = false
        };

        // Evita volver a intentar cambios pendientes de la operación que falló.
        _dbContext.ChangeTracker.Clear();
        _dbContext.ErrorLogs.Add(error);
    }

    private static string? Limitar(string? valor, int maximo)
    {
        if (string.IsNullOrEmpty(valor))
        {
            return valor;
        }

        return valor.Length <= maximo ? valor : valor[..maximo];
    }
}
