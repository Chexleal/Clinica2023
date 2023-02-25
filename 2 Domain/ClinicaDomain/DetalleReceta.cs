using System;
using System.Collections.Generic;

namespace clinicaWeb.Models;

public partial class DetalleReceta
{
    public decimal IdDetalleReceta { get; set; }

    public decimal? IdReceta { get; set; }

    public string Medicamento { get; set; } = null!;

    public string? DosisDia { get; set; }

    public string? DosisTiempo { get; set; }

    public string? Instrucciones { get; set; }

    public decimal? CantidadMed { get; set; }

    public virtual Receta? IdRecetaNavigation { get; set; }
}
