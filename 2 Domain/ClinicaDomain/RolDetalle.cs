﻿using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class RolDetalle
{
    public Guid IdRolDetalle { get; set; }

    public Guid UsuarioId { get; set; }

    public string Permiso { get; set; }

    public string Descripcion { get; set; }

}
