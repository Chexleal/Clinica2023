using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class RolDetalle
{
    public Guid IdRolDetalle { get; set; }

    public Guid? IdUsuario { get; set; }

    public string Permiso { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
