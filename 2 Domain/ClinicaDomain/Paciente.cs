
using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class Paciente
{
    public Guid IdPaciente { get; set; }

    public string Dpi { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public DateTime FechaNacimiento { get; set; }

    public string Genero { get; set; } = null!;

    public int Telefono { get; set; }

    public string? Correo { get; set; }

    public string? Direccion { get; set; }

    public string? Alergias { get; set; }

    public string EstadoCivil { get; set; } = null!;

    public string Profesion { get; set; } = null!;

    public string Nacionalidad { get; set; } = null!;

    public string Remitido { get; set; } = null!;

    public string Antecedentes { get; set; } = null!;

    public string TipoSange { get; set; } = null!;

    public bool EstadoEliminado { get; set; }

    public string NoRegistro { get; set; } = null!;

    public virtual ICollection<Cita> Cita { get; } = new List<Cita>();

    public virtual ICollection<Consulta> Consulta { get; } = new List<Consulta>();
}
