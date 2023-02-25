using System;
using System.Collections.Generic;

namespace clinicaWeb.Models;

public partial class RolDetalle
{
    public decimal IdRolDetalle { get; set; }

    public decimal? IdPermiso { get; set; }

    public string? Descripcion { get; set; }

    public decimal? IdUser { get; set; }

    public virtual Permiso? IdPermisoNavigation { get; set; }

    public virtual Usuario? IdUserNavigation { get; set; }
}
