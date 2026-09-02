namespace ClinicaDomain;

public class ErrorLog : Base
{
    public Guid IdErrorLog { get; set; }
    public string TipoError { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public string? StackTrace { get; set; }
    public string? Ruta { get; set; }
    public string? MetodoHttp { get; set; }
    public string? TraceIdentifier { get; set; }
    public string Nivel { get; set; } = "Error";
    public bool Resuelto { get; set; }
    public string? Observaciones { get; set; }
}
