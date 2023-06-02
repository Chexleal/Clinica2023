using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;

namespace ClinicaDomain;

public partial class Usuario
{

    public Guid IdUsuario { get; set; }

    public string NombreUsuario { get; set; }

    public string PreguntaSeg { get; set; }

    public string RespuestaSeg { get; set; }

    public string Dpi { get; set; }

    public string Nombre { get; set; }

    public string Apellido { get; set; }

    public DateTime FechaNacimiento { get; set; }

    public int Telefono { get; set; }

    public string Correo { get; set; }

    public bool UsuarioActivo { get; set; }

    public string EstadoCivil { get; set; }

    public string Profesion { get; set; }

    public string Nacionalidad { get; set; }

    public string Antecedentes { get; set; }

    public string TipoSange { get; set; }

    public bool EstadoEliminado { get; set; }

    public string Password { get; set; }

    //public virtual ICollection<Cita> Cita { get; } = new List<Cita>();

    public  List<RolDetalle> Permisos { get; set; } = new List<RolDetalle>();

    public void BeforeChanges()
    {
        NombreUsuario ??= string.Empty;
        PreguntaSeg ??= string.Empty;
        RespuestaSeg ??= string.Empty;
        Dpi ??= string.Empty;
        Nombre ??= string.Empty;
        Apellido ??= string.Empty;
        Correo ??= string.Empty;
        EstadoCivil ??= string.Empty;
        Profesion ??= string.Empty;
        Nacionalidad ??= string.Empty;
        Antecedentes ??= string.Empty;
        TipoSange ??= string.Empty;
        Password ??= string.Empty;
    }


}
