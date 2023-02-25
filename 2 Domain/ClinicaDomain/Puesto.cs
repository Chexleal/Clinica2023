using System;
using System.Collections.Generic;

namespace clinicaWeb.Models;

public partial class Puesto
{
    public decimal IdPuesto { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Empleado> Empleados { get; } = new List<Empleado>();
}
