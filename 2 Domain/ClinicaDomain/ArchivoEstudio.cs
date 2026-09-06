namespace ClinicaDomain;

public partial class ArchivoEstudio : Base
{
    public Guid IdArchivo { get; set; }
    public Guid IdEstudio { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public string RutaStorage { get; set; } = string.Empty; // ej: estudios/{paciente}/{estudio}/{guid}.dcm
    public string MimeType { get; set; } = string.Empty; // application/dicom
    public long TamanoBytes { get; set; }
    public string? TransferSyntax { get; set; }
    public int? NumeroSerie { get; set; }
    public int? NumeroInstancia { get; set; }

    public virtual EstudioImagen Estudio { get; set; } = null!;
}
