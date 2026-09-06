namespace ClinicaDomain;

public enum TipoEstudio
{
    Rx = 1,
    Panoramica = 2,
    Cefalometrica = 3,
    Tomografia = 4,
    Resonancia = 5,
    Ultrasonido = 6,
    Laboratorio = 7,
    Otro = 99
}

public enum ModalidadDicom
{
    CR = 1, // Computed Radiography
    DX = 2, // Digital Radiography
    CT = 3,
    MR = 4,
    US = 5,
    OT = 6  // Other
}

public enum EstadoOrden
{
    Pendiente = 0,
    EnProceso = 1,
    Completada = 2,
    Cancelada = 3
}
