using System;
using System.Collections.Generic;

namespace clinicaWeb.Models;

public partial class Usuario
{
    public decimal IdUser { get; set; }

    public string? Password { get; set; }

    public string? UsuarioNombre { get; set; }

    public string? PreguntaSeg { get; set; }

    public string? RespuestaSeg { get; set; }

    public string? Huella { get; set; }

    public decimal? IdEmpleado { get; set; }

    public virtual Empleado? IdEmpleadoNavigation { get; set; }

    public virtual ICollection<RolDetalle> RolDetalles { get; } = new List<RolDetalle>();
}
