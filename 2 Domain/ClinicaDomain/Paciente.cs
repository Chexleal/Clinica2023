using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class Paciente
{
   

    public Guid IdPaciente { get; set; }

    public string Dpi { get; set; } 

    public string Nombre { get; set; } 

    public string Apellido { get; set; } 

    public DateTime FechaNacimiento { get; set; }

    public string Genero { get; set; } 

    public string Telefono { get; set; } 

    public string Correo { get; set; } 

    public string Direccion { get; set; } 

    public string Alergias { get; set; }

    public string EstadoCivil { get; set; } 

    public string Profesion { get; set; }

    public string Nacionalidad { get; set; } 

    public string Remitido { get; set; } 

    public string Antecedentes { get; set; }

    public string TipoSange { get; set; } 

    public bool EstadoEliminado { get; set; }

    public string NoRegistro { get; set; } 

  //  public ICollection<Cita> Cita { get; set; } = new List<Cita>();

    public ICollection<Consulta> Consulta { get; set; } = new List<Consulta>();

    public void BeforeSaveChanges()
    {
        Dpi ??= string.Empty;
        Nombre ??= string.Empty;
        Apellido ??= string.Empty;
        Genero ??= string.Empty;
        Correo ??= string.Empty;
        Direccion ??= string.Empty;
        Alergias ??= string.Empty;
        EstadoCivil ??= string.Empty;
        Profesion ??= string.Empty;
        Nacionalidad ??= string.Empty;
        Remitido ??= string.Empty;
        Antecedentes ??= string.Empty;
        TipoSange ??= string.Empty;
        NoRegistro ??= string.Empty;
        Telefono??=string.Empty;
    }
}
