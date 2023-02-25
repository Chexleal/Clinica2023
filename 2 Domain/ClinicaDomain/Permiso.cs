using System;
using System.Collections.Generic;

namespace clinicaWeb.Models;

public partial class Permiso
{
    public decimal IdPermiso { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<RolDetalle> RolDetalles { get; } = new List<RolDetalle>();
}
