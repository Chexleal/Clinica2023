using System;
using System.Collections.Generic;

namespace clinicaWeb.Models;

public partial class Paciente
{
    public decimal IdPaciente { get; set; }

    public string? Dpi { get; set; }

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public DateTime? FechaNac { get; set; }

    public string? Genero { get; set; }

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public string? Direccion { get; set; }

    public string? Alergias { get; set; }

    public string? EstadoCivil { get; set; }

    public string? Profesion { get; set; }

    public string? Nacionalidad { get; set; }

    public string? Remitido { get; set; }

    public string? Antecedentes { get; set; }

    public string? TipoSangre { get; set; }

    public bool? EstadoEliminado { get; set; }

    public string? NoRegistro { get; set; }

    public virtual ICollection<Consulta> Consulta { get; } = new List<Consulta>();
}
