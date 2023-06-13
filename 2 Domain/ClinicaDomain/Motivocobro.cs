﻿using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class MotivoCobro
{
    public Guid IdMotivoCobro { get; set; }

    public string Descripcion { get; set; }

    public bool EstadoEliminado { get; set; }

    //public virtual ICollection<DetalleCobro> DetalleCobros { get; } = new List<DetalleCobro>();

    public void BeforeSaveChanges()
    {
        Descripcion ??= string.Empty;
    }
}
