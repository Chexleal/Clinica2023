using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class Receta
{
    public Guid IdReceta { get; set; }

    public Guid IdConsulta { get; set; }

    public DateTime Fecha { get; set; }

    public virtual ICollection<DetalleReceta> DetalleReceta { get; } = new List<DetalleReceta>();
    public string Descripcion { get; set; }

    //public virtual Consulta IdConsultaNavigation { get; set; }
}
