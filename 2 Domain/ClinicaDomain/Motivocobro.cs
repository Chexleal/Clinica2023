using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class MotivoCobro : Base, IHospitalTenant
{
    public Guid IdMotivoCobro { get; set; }

    public Guid IdHospital { get; set; }
    public Guid? IdCategoriaProducto { get; set; }

    public string Descripcion { get; set; }

    public string? Area { get; set; }

    public bool EstadoEliminado { get; set; }

    public decimal PrecioSugerido { get; set; }
    /// <summary>Precio general excepcional para emergencias; una tarifa de aseguradora tiene precedencia.</summary>
    public decimal? PrecioEmergenciaGeneral { get; set; }

    //public virtual ICollection<DetalleCobro> DetalleCobros { get; } = new List<DetalleCobro>();

    public void BeforeSaveChanges()
    {
        Descripcion ??= string.Empty;
    }
}
