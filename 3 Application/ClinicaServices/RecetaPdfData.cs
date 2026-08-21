using ClinicaDomain;
using System.Collections.Generic;

namespace ClinicaServices;

public record RecetaPdfData(
    Receta Receta,
    Consulta Consulta,
    Paciente Paciente,
    List<DetalleReceta> DetallesReceta,
    DateTime CitaProx
);