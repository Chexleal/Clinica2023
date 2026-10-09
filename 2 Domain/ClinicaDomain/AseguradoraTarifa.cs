namespace ClinicaDomain;

/// <summary>Tarifa pactada con una aseguradora para un producto o servicio.</summary>
public class AseguradoraTarifa : Base, IHospitalTenant
{
    public Guid IdAseguradoraTarifa { get; set; }
    public Guid IdHospital { get; set; }
    public Guid IdAseguradora { get; set; }
    public Guid? IdProducto { get; set; }
    public Guid? IdMotivoCobro { get; set; }
    /// <summary>Precio pactado por concepto para atención normal.</summary>
    public decimal PrecioConvenido { get; set; }
    public decimal? PrecioEmergencia { get; set; }
    public bool Activa { get; set; } = true;
}
