using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class DetalleReceta
{
    public Guid IdDetalleReceta { get; set; }

    public Guid IdReceta { get; set; }

    public string Medicamento { get; set; }
    public string DosisDias { get; set; }
    public string DosisTiempo { get; set; }
    public string Instrucciones { get; set; }
    public string Cantidad { get; set; }

    //public virtual Receta IdRecetaNavigation { get; set; }
}
