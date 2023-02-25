using System;
using System.Collections.Generic;

namespace clinicaWeb.Models;

public partial class Empleado
{
    public decimal IdEmpleado { get; set; }

    public string? Dpi { get; set; }

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public DateTime? FechaNac { get; set; }

    public decimal? Telefono { get; set; }

    public string? Correo { get; set; }

    public string? Direccion { get; set; }

    public decimal? IdPuesto { get; set; }

    public decimal? UserExist { get; set; }

    public decimal? IdClinica { get; set; }

    public string? Foto { get; set; }

    public virtual ICollection<Consulta> Consulta { get; } = new List<Consulta>();

    public virtual Puesto? IdPuestoNavigation { get; set; }

    public virtual ICollection<Usuario> Usuarios { get; } = new List<Usuario>();
}
