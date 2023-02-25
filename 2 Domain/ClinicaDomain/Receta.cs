using System;
using System.Collections.Generic;

namespace clinicaWeb.Models;

public partial class Receta
{
    public decimal IdReceta { get; set; }

    public decimal? IdConsulta { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual ICollection<DetalleReceta> DetalleReceta { get; } = new List<DetalleReceta>();

    public virtual Consulta? IdConsultaNavigation { get; set; }
}
