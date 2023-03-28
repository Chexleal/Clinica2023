
using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class DetalleCobro
{
    public Guid IdDetalleCobro { get; set; }

    public Guid? IdConsulta { get; set; }

    public Guid? IdMotivoCobro { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Valor { get; set; }

    public string Producto { get; set; } = null!;

    public virtual Consulta? IdConsultaNavigation { get; set; }

    public virtual MotivoCobro? IdMotivoCobroNavigation { get; set; }
}
