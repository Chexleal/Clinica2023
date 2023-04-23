using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class DetalleReceta
{
    public Guid IdDetalleReceta { get; set; }

    public Guid IdReceta { get; set; }

    public string Descripcion { get; set; }

    public virtual Receta IdRecetaNavigation { get; set; }
}
