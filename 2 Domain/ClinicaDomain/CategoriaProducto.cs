namespace ClinicaDomain;

public partial class CategoriaProducto : Base, IHospitalTenant
{
    public Guid IdCategoriaProducto { get; set; }

    public Guid IdHospital { get; set; }

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Bien | Servicio. Bien mueve inventario; ambos tipos clasifican cargos del estado de cuenta.</summary>
    public string Tipo { get; set; } = "Bien";

    /// <summary>Orden en listados e impresión del estado de cuenta.</summary>
    public int Orden { get; set; } = 99;

    /// <summary>Valores por defecto al crear un producto en esta categoría. Opcionales, editables por producto.</summary>
    public bool ExigeLoteDefault { get; set; }

    public bool ExigeVencimientoDefault { get; set; }

    public bool Activo { get; set; } = true;

    public void BeforeSaveChanges()
    {
        Nombre ??= string.Empty;
        Tipo ??= "Bien";
    }
}
