using System;
using System.Collections.Generic;

namespace clinicaWeb.Models;

public partial class Detallecobro
{
    public decimal IdDetalleCobro { get; set; }

    public decimal? IdConsulta { get; set; }

    public decimal? IdMotivoCobro { get; set; }

    public decimal? SubTotal { get; set; }

    public decimal? Valor { get; set; }

    public string? Producto { get; set; }

    public virtual Motivocobro? IdMotivoCobroNavigation { get; set; }
}
