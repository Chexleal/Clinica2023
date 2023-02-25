using System;
using System.Collections.Generic;

namespace clinicaWeb.Models;

public partial class Motivocobro
{
    public decimal IdMotivoCobro { get; set; }

    public string? Descripción { get; set; }

    public virtual ICollection<Detallecobro> Detallecobros { get; } = new List<Detallecobro>();
}
