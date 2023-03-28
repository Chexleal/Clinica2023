
using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class Usuario
{
    public Guid IdUsuario { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string PreguntaSeg { get; set; } = null!;

    public string RespuestaSeg { get; set; } = null!;

    public string Dpi { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public DateTime FechaNacimiento { get; set; }

    public int Telefono { get; set; }

    public string Correo { get; set; } = null!;

    public bool UsuarioActivo { get; set; }

    public string EstadoCivil { get; set; } = null!;

    public string Profesion { get; set; } = null!;

    public string Nacionalidad { get; set; } = null!;

    public string Remitido { get; set; } = null!;

    public string Antecedentes { get; set; } = null!;

    public string TipoSange { get; set; } = null!;

    public bool EstadoEliminado { get; set; }

    public string NoRegistro { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Cita> Cita { get; } = new List<Cita>();

    public virtual ICollection<RolDetalle> RolDetalles { get; } = new List<RolDetalle>();
}
